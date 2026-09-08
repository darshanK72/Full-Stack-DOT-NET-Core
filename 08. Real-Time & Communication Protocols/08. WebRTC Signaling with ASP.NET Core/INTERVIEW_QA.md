# WebRTC Signaling with ASP.NET Core — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — WebRTC Signaling with ASP.NET Core (Interview Traps)](#gotchas--webrtc-signaling-with-aspnet-core-interview-traps)

---

## Gotchas — WebRTC Signaling with ASP.NET Core (Interview Traps)

---

#### Gotcha 1. ICE Candidates Must Be Sent Incrementally via Trickle ICE — Waiting for Full Gathering Is Too Slow

**Concepts**
- Trickle ICE sends each candidate to the remote peer as soon as it is discovered
- Waiting for onicegatheringstatechange === "complete" before sending any candidates adds seconds of latency
- RTCPeerConnection.onicecandidate fires per candidate; each must be relayed immediately
- The remote peer must call addIceCandidate() for each received candidate as it arrives

**Answer**

WebRTC's ICE agent discovers network candidate addresses through three mechanisms: local interface enumeration (fast), STUN server queries (hundreds of milliseconds), and TURN relay allocation (seconds). Trickle ICE sends each candidate to the remote peer via the signaling channel the moment it is discovered, allowing the peers to begin connectivity checks immediately. Waiting for `onicegatheringstatechange === "complete"` before sending any candidates causes the entire gathering phase — which can take several seconds on slow or restrictive networks — to complete before connection attempts even begin, adding noticeable delay to call setup. The `onicecandidate` handler must relay each candidate object to the remote peer through the ASP.NET Core SignalR hub as soon as it fires, and the remote peer must call `pc.addIceCandidate(candidate)` on each received candidate immediately.

---

#### Gotcha 2. STUN Is Insufficient for Symmetric NAT — TURN Is Required but Often Forgotten in Production

**Concepts**
- STUN discovers the public IP:port of a client behind NAT (server-reflexive candidates)
- Symmetric NAT maps each connection to a different external port, defeating STUN-based candidates
- TURN relays media through the TURN server when direct connectivity fails
- Running without TURN means calls fail for a significant percentage of real-world users

**Answer**

A STUN server allows a WebRTC client to discover its externally visible IP address and port (server-reflexive candidate). This works for full-cone, restricted-cone, and port-restricted NAT types. Symmetric NAT, which is common in corporate networks and some carrier-grade NATs, assigns a different external port for each destination address, making the STUN-discovered candidate useless for direct peer-to-peer connectivity. TURN provides a media relay path that works regardless of NAT type: both peers connect to the TURN server and it forwards media between them. Applications that deploy only STUN (using the free Google STUN servers) will have WebRTC calls fail for any user behind symmetric NAT. TURN servers carry media traffic and must be properly sized and secured with credentials (`iceTransportPolicy` set to `"relay"` forces TURN for testing).

---

#### Gotcha 3. The Signaling Server Is Not in the Media Path After ICE — Confusing Signaling Capacity with Media Capacity

**Concepts**
- After ICE completes, media flows peer-to-peer and the signaling server handles zero media bytes
- The signaling server's load scales with the number of connections, not the number of concurrent media sessions
- Only TURN relay traffic passes through a server — purely a NAT traversal fallback
- Sizing the signaling server for media bandwidth is a category error

**Answer**

WebRTC's value proposition is that once ICE negotiation succeeds and DTLS-SRTP is established, audio and video frames travel directly between clients over UDP without touching any server. The signaling server (the ASP.NET Core SignalR hub) only handles the short bootstrapping phase — SDP offer/answer exchange and ICE candidate relay — and then carries only application-level control events (mute, leave, roster updates) for the rest of the call. The signaling server's resource requirements scale with the number of active connections and the rate of control messages, not with media bitrate or call duration. A common architectural mistake is allocating expensive high-bandwidth server capacity for signaling and underestimating TURN relay costs — TURN is where media bandwidth actually flows when direct P2P paths are unavailable.

---

#### Gotcha 4. SDP Offer and Answer Must Not Be Modified by the Signaling Server

**Concepts**
- SDP (Session Description Protocol) is a text document describing codecs, ICE credentials, DTLS fingerprints
- Any modification to the SDP by the signaling server corrupts the cryptographic fingerprint or codec negotiation
- The signaling server must relay SDP blobs verbatim without parsing or transforming them
- Server-side SDP manipulation is the domain of an SFU (Selective Forwarding Unit), not a signaling server

**Answer**

SDP contains a `a=fingerprint:` attribute with the DTLS certificate fingerprint that the receiving peer uses to verify the remote endpoint's identity. It also contains ICE credentials (`a=ice-ufrag:`, `a=ice-pwd:`) and a precise codec capability description. Any modification to the SDP by an intermediary — even whitespace normalisation or JSON re-serialisation of the string — can corrupt these values. The DTLS handshake will fail if the fingerprint is altered, and ICE will fail if the credentials are changed. The ASP.NET Core SignalR hub acting as signaling relay must treat the SDP as an opaque string and forward it byte-for-byte. SDP transformation belongs to SFU (Selective Forwarding Unit) infrastructure like mediasoup or Janus, which are separate specialised components.

---

#### Gotcha 5. addIceCandidate Must Not Be Called Before setRemoteDescription

**Concepts**
- RTCPeerConnection requires the remote SDP to be set before ICE candidates can be applied
- Trickle ICE delivers candidates while SDP exchange is still in flight
- Candidates received before setRemoteDescription must be queued and applied after
- Calling addIceCandidate before setRemoteDescription throws InvalidStateError

**Answer**

Trickle ICE delivers ICE candidates from the remote peer asynchronously, and they may arrive at the signaling channel before the remote peer's SDP answer has been received and applied with `setRemoteDescription`. Calling `pc.addIceCandidate(candidate)` before `setRemoteDescription` has completed throws `InvalidStateError: Can't add ICE candidate without a remote description`. An application that calls `addIceCandidate` immediately upon receiving a candidate from the signaling hub will hit this error whenever network timing causes a candidate message to arrive before the answer message. The fix is to maintain a client-side queue of received candidates: if `pc.remoteDescription` is null at the time a candidate arrives, push the candidate into the queue; once `setRemoteDescription` completes, drain the queue by calling `addIceCandidate` for each queued candidate.

---

#### Gotcha 6. Re-negotiation Is Required When Tracks Are Added or Removed After Initial Connection

**Concepts**
- Adding a track after the initial offer/answer exchange requires a new SDP re-negotiation
- RTCPeerConnection.onnegotiationneeded fires when re-negotiation is required
- Failure to re-negotiate after addTrack() means the remote peer never receives the new track
- Screen share, dynamic audio tracks, and camera switches all trigger re-negotiation

**Answer**

The initial SDP offer/answer exchange describes the media capabilities and track structure for the connection. Adding a new track — enabling screen sharing, switching cameras, adding a second audio source — after the initial negotiation changes the local media description and fires `RTCPeerConnection.onnegotiationneeded`. The application must respond to this event by creating a new SDP offer, setting it as the local description, sending it to the remote peer via the signaling channel, and completing a new offer/answer cycle. An application that calls `pc.addTrack(track)` without handling `onnegotiationneeded` will successfully add the track locally but the remote peer will never receive it because the SDP describing the new track was never exchanged. Screen share is the most commonly forgotten trigger for re-negotiation.

---

#### Gotcha 7. WebRTC Data Channels Have Different Reliability Modes — Choosing Wrong Causes Packet Loss or Reordering

**Concepts**
- Reliable ordered: all messages delivered in order (like TCP); default mode
- Reliable unordered: all messages delivered but possibly out of order
- Unreliable: messages may be dropped; like UDP; suitable for real-time game state
- Data channels and media tracks are independent; adding a data channel requires re-negotiation

**Answer**

WebRTC data channels support three reliability modes configured at channel creation: reliable ordered (the default, behaves like TCP), reliable unordered (all messages delivered but delivery order is not guaranteed), and unreliable with an optional maximum retransmit count or maximum packet lifetime (messages may be dropped, suitable for real-time data where freshness matters more than completeness). Choosing reliable ordered for high-frequency real-time telemetry — mouse positions, game state, live sensor readings — causes head-of-line blocking: a dropped packet stalls all subsequent messages waiting for retransmission, increasing latency. Choosing unreliable for critical data like chat messages silently drops messages under congestion. The reliability mode is set at channel creation and cannot be changed afterward, so the choice must be made deliberately based on the application's data semantics.

---

#### Gotcha 8. The Signaling Channel Must Be Bidirectional — SSE Alone Cannot Complete WebRTC Bootstrapping

**Concepts**
- SDP offer must travel from caller to callee; SDP answer must travel from callee to caller
- ICE candidates must travel in both directions simultaneously
- SSE provides only server-to-client delivery; client-to-server requires separate POST requests
- A SignalR hub or raw WebSocket is the natural choice for bidirectional signaling

**Answer**

WebRTC bootstrapping requires bidirectional signaling: the caller sends an SDP offer to the callee, the callee sends an SDP answer back to the caller, and both peers continuously send ICE candidates to each other as they are discovered. SSE provides only server-to-client delivery. A signaling implementation built on SSE must pair it with separate HTTP POST requests for each client-to-server message (offer, answer, ICE candidates). This is architecturally cumbersome — each POST requires a new TCP connection on HTTP/1.1 and adds round-trip overhead at the most latency-sensitive phase of call setup. A SignalR hub or raw WebSocket provides natural bidirectionality and is the standard choice for WebRTC signaling in ASP.NET Core applications. The SignalR hub relays `offer`, `answer`, and `icecandidate` messages between two specific clients identified by their connection IDs or a room identifier.

---

#### Gotcha 9. DTLS Fingerprint Mismatch Causes Silent Connection Failure at the Media Layer

**Concepts**
- DTLS fingerprint in SDP authenticates the remote DTLS certificate at key exchange time
- Fingerprint mismatch aborts the DTLS handshake without a user-visible error in most browsers
- A wrong fingerprint can result from SDP modification, server-side SDP rewriting, or proxy interference
- The RTCPeerConnection iceconnectionstatechange will show "failed" with no explanation

**Answer**

The `a=fingerprint:` attribute in the SDP contains a hash of the peer's DTLS certificate. During the DTLS handshake that establishes the SRTP keys, each peer verifies the remote certificate against the fingerprint in the SDP. If the fingerprint does not match — because the SDP was modified in transit, a signaling relay changed the encoding, or two separate peer connections inadvertently used the same SDP with mismatched certificates — the DTLS handshake silently fails. The `RTCPeerConnection` transitions to `iceConnectionState = "failed"` but provides no detailed error to JavaScript. The `RTCPeerConnection.getStats()` API can reveal DTLS failure details in the connection statistics. When diagnosing unexplained connection failures after ICE candidates are exchanged, DTLS fingerprint mismatch should be an early suspect, especially if any component in the signaling path modifies the SDP.

---

#### Gotcha 10. Perfect Negotiation Pattern Must Be Implemented to Handle Glare (Simultaneous Offers)

**Concepts**
- Glare occurs when both peers create offers simultaneously, triggering a negotiation conflict
- Without perfect negotiation, both peers may set an offer as remote description and get an error
- Perfect negotiation designates one peer as polite (rolls back local offer) and one as impolite (ignores remote offer)
- onnegotiationneeded can fire multiple times; using a makeOffer flag prevents concurrent offer creation

**Answer**

WebRTC glare occurs when both peers simultaneously call `createOffer` and send their SDP offers to each other before either has sent an answer. Each peer receives the other's offer while already in the `have-local-offer` state. Without coordination, both peers attempt to set the incoming offer as the remote description, which fails because the local state machine is not in the correct state. The "perfect negotiation" pattern resolves glare by designating one peer as the "polite" peer: when the polite peer receives a remote offer while it has a pending local offer, it rolls back its local offer (`setLocalDescription({type: "rollback"})`) and processes the remote offer instead. The "impolite" peer ignores an incoming offer when it already has a pending local offer. The ASP.NET Core signaling hub enables perfect negotiation by passing a `polite` flag with each connection's room-join confirmation so each client knows its role.

---
