# Kubernetes Orchestration — Interview Q&A
> 29 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Kubernetes, and what problems does it solve for running containerized microservices at scale?](#q1-what-is-kubernetes-and-what-problems-does-it-solve-for-running-containerized-microservices-at-scale)
2. [Q2. What are the main components of the Kubernetes control plane, and what does each one do?](#q2-what-are-the-main-components-of-the-kubernetes-control-plane-and-what-does-each-one-do)
3. [Q3. What runs on each worker node in a Kubernetes cluster?](#q3-what-runs-on-each-worker-node-in-a-kubernetes-cluster)
4. [Q4. What is kubectl, and how does it communicate with the cluster?](#q4-what-is-kubectl-and-how-does-it-communicate-with-the-cluster)
5. [Q5. What is a Kubernetes Namespace, and when would you use multiple namespaces in a single cluster?](#q5-what-is-a-kubernetes-namespace-and-when-would-you-use-multiple-namespaces-in-a-single-cluster)
6. [Q6. What is a Pod in Kubernetes, and why is it the smallest deployable unit rather than a container?](#q6-what-is-a-pod-in-kubernetes-and-why-is-it-the-smallest-deployable-unit-rather-than-a-container)
7. [Q7. What is a Deployment, and what does it manage on your behalf?](#q7-what-is-a-deployment-and-what-does-it-manage-on-your-behalf)
8. [Q8. What is a ReplicaSet, and what role does it play during a Deployment rollout?](#q8-what-is-a-replicaset-and-what-role-does-it-play-during-a-deployment-rollout)
9. [Q9. What is the difference between a RollingUpdate and a Recreate deployment strategy?](#q9-what-is-the-difference-between-a-rollingupdate-and-a-recreate-deployment-strategy)
10. [Q10. What is a StatefulSet, and when would you use one instead of a Deployment?](#q10-what-is-a-statefulset-and-when-would-you-use-one-instead-of-a-deployment)
11. [Q11. Why does Kubernetes use Services to expose Pods rather than using Pod IP addresses directly?](#q11-why-does-kubernetes-use-services-to-expose-pods-rather-than-using-pod-ip-addresses-directly)
12. [Q12. What is the difference between ClusterIP, NodePort, and LoadBalancer Service types?](#q12-what-is-the-difference-between-clusterip-nodeport-and-loadbalancer-service-types)
13. [Q13. What is an Ingress resource, and how does it differ from a Service of type LoadBalancer?](#q13-what-is-an-ingress-resource-and-how-does-it-differ-from-a-service-of-type-loadbalancer)
14. [Q14. How does DNS-based service discovery work for inter-service communication within a cluster?](#q14-how-does-dns-based-service-discovery-work-for-inter-service-communication-within-a-cluster)
15. [Q15. What is a ConfigMap, and how do you inject configuration from one into a Pod?](#q15-what-is-a-configmap-and-how-do-you-inject-configuration-from-one-into-a-pod)
16. [Q16. What is a Kubernetes Secret, and how does it differ from a ConfigMap in terms of security?](#q16-what-is-a-kubernetes-secret-and-how-does-it-differ-from-a-configmap-in-terms-of-security)
17. [Q17. What is the difference between a PersistentVolume and a PersistentVolumeClaim?](#q17-what-is-the-difference-between-a-persistentvolume-and-a-persistentvolumeclaim)
18. [Q18. When does a .NET microservice need a PersistentVolume, and when can it avoid one?](#q18-when-does-a-net-microservice-need-a-persistentvolume-and-when-can-it-avoid-one)
19. [Q19. What is the difference between a liveness probe and a readiness probe in Kubernetes?](#q19-what-is-the-difference-between-a-liveness-probe-and-a-readiness-probe-in-kubernetes)
20. [Q20. How do you configure liveness and readiness probes for an ASP.NET Core application?](#q20-how-do-you-configure-liveness-and-readiness-probes-for-an-aspnet-core-application)
21. [Q21. What is a startup probe, and when should you add one alongside a liveness probe?](#q21-what-is-a-startup-probe-and-when-should-you-add-one-alongside-a-liveness-probe)
22. [Q22. What are resource requests and limits, and how do they affect .NET container behavior?](#q22-what-are-resource-requests-and-limits-and-how-do-they-affect-net-container-behavior)
23. [Q23. What is Horizontal Pod Autoscaling, and what metrics does it use by default?](#q23-what-is-horizontal-pod-autoscaling-and-what-metrics-does-it-use-by-default)
24. [Q24. How do you write a multi-stage Dockerfile for an ASP.NET Core application suitable for Kubernetes?](#q24-how-do-you-write-a-multi-stage-dockerfile-for-an-aspnet-core-application-suitable-for-kubernetes)
25. [Q25. How does graceful shutdown work in a .NET application running inside a Kubernetes Pod?](#q25-how-does-graceful-shutdown-work-in-a-net-application-running-inside-a-kubernetes-pod)
26. [Q26. What environment variable naming convention must you follow to expose hierarchical ASP.NET Core configuration from a Kubernetes ConfigMap or Secret?](#q26-what-environment-variable-naming-convention-must-you-follow-to-expose-hierarchical-aspnet-core-configuration-from-a-kubernetes-configmap-or-secret)
27. [Q27. What is Helm, and why is it used instead of raw YAML manifests for production Kubernetes deployments?](#q27-what-is-helm-and-why-is-it-used-instead-of-raw-yaml-manifests-for-production-kubernetes-deployments)
28. [Q28. How does a Kubernetes rolling deployment achieve zero-downtime updates, and what role does the readiness probe play?](#q28-how-does-a-kubernetes-rolling-deployment-achieve-zero-downtime-updates-and-what-role-does-the-readiness-probe-play)
29. [Q29. What is a PodDisruptionBudget, and why should production deployments define one?](#q29-what-is-a-poddisruptionbudget-and-why-should-production-deployments-define-one)

---

## Q1. What is Kubernetes, and what problems does it solve for running containerized microservices at scale?

**Concepts**
- Helm chart as versioned package of parameterized Kubernetes resource definitions
- values.yaml and environment-specific override files
- helm upgrade --install as idempotent CI/CD deploy command
- helm rollback for Helm-tracked revision history
- Kustomize as templating-free overlay alternative

**Answer**

Helm is the package manager for Kubernetes. It bundles related Kubernetes resource definitions — Deployment, Service, ConfigMap, Ingress — into a reusable, versioned package called a chart, parameterized with a values file so the same chart can be deployed to different environments by overriding a small set of variables. Without Helm, teams maintain separate copies of nearly identical YAML files for each environment, which drifts and becomes inconsistent over time. A Helm chart contains Go-templated YAML files in a `templates/` directory, a `Chart.yaml` with metadata and version, and a `values.yaml` with default parameter values; environment overrides are applied via `-f prod-values.yaml` at install time. `helm upgrade --install <release> <chart>` is idempotent — it installs on first run and upgrades on subsequent runs, making it safe to run from a CI/CD pipeline on every push. `helm rollback <release> <revision>` rolls the deployment back to a previous Helm release revision, wrapping `kubectl rollout undo` with full Helm history tracking. Kustomize is a common alternative that uses overlays and patches without templating — it is built into `kubectl apply -k` and preferred when templating complexity is not needed. Many teams use both: Helm for packaging third-party software like cert-manager and Nginx Ingress, and Kustomize for their own application configuration.

---

## Q2. What are the main components of the Kubernetes control plane, and what does each one do?

**Concepts**
- maxUnavailable: 0 with maxSurge: 1 for zero-downtime rolling update
- Readiness probe gating — rollout stalls rather than removes old pods when new pod fails
- preStop lifecycle hook to drain endpoint references before SIGTERM
- PodDisruptionBudget complementing rolling update during node drains

**Answer**

A rolling update replaces old Pods with new ones incrementally, bounded by the `maxSurge` and `maxUnavailable` settings. Zero-downtime is achieved when `maxUnavailable` is set to zero and the readiness probe is correctly implemented, because Kubernetes only routes traffic to a new Pod after its readiness probe passes and only terminates an old Pod after the new one is ready. With `maxUnavailable: 0` and `maxSurge: 1`, Kubernetes starts one new Pod, waits for its readiness probe to pass, then removes one old Pod — and so on until all replicas are updated, so at no point does the number of ready Pods fall below the desired replica count. If the new image fails its readiness probe because the application has a startup bug, the rollout stalls and the old Pods remain running and receiving traffic — the rollout does not proceed to remove old Pods, which is an automatic safety gate. A `preStop` lifecycle hook that sleeps for a few seconds on old Pods before they receive SIGTERM gives the Service endpoint list time to propagate the Pod's removal, preventing connection errors from clients that still hold stale references to the old Pod IP. PodDisruptionBudgets complement this by preventing more Pods than `maxUnavailable` allows from being voluntarily disrupted simultaneously during events like node drains.

---

## Q3. What runs on each worker node in a Kubernetes cluster?

**Concepts**
- PDB limiting voluntary disruptions during node drains and cluster upgrades
- minAvailable vs maxUnavailable as two expression forms
- Voluntary vs involuntary disruption distinction
- PDB independence from rolling update maxUnavailable setting

**Answer**

A PodDisruptionBudget (PDB) is a Kubernetes policy that limits the number of Pods belonging to a workload that can be voluntarily disrupted at the same time. Voluntary disruptions include node drains for cluster upgrades, maintenance, or scaling down, evictions for resource pressure, and disruptions triggered by cluster autoscalers. Without a PDB, a cluster upgrade could evict all Pods of a service simultaneously, causing an outage. A PDB is expressed either as `minAvailable` — the minimum number of Pods that must remain up — or `maxUnavailable` — the maximum number that can be disrupted simultaneously. For a service with three replicas and `minAvailable: 2`, at most one Pod can be disrupted at a time. PDBs only protect against voluntary disruptions — they cannot prevent a node from failing involuntarily. If a node crashes, Kubernetes reschedules the Pods regardless of the PDB. A PDB of `minAvailable: 1` is a common baseline: it ensures the service stays online during node drains while still allowing the cluster to proceed with maintenance. Setting `minAvailable` equal to the replica count blocks all disruptions and will stall cluster upgrades indefinitely. PDBs work in conjunction with rolling update settings but are independent: rolling update `maxUnavailable` controls updates triggered by Deployment changes, while the PDB controls disruptions triggered externally by the cluster control plane.

---

## Q4. What is kubectl, and how does it communicate with the cluster?

**Concepts**
- Resource request as scheduler placement guarantee
- Memory limit OOMKill vs CPU limit throttling difference
- CPU limit caution for .NET thread pool and GC behavior
- DOTNET_GCConserveMemory for memory-constrained containers

**Answer**

A resource request is the minimum CPU and memory the container is guaranteed to receive; the Kubernetes scheduler uses requests to decide which node can host a Pod. A resource limit is the maximum the container may consume — exceeding the memory limit causes the container to be killed (OOMKilled) and restarted, while exceeding the CPU limit results in throttling (slower execution, not a restart). For .NET applications, memory sizing must account for the .NET runtime itself, the managed heap, native memory used by the GC and thread stacks, and any third-party libraries that allocate native memory — a common starting point is 256 Mi request and 512 Mi limit for a small API, monitored and tuned from real load data. CPU limits for .NET containers require care because the .NET thread pool sizes itself based on the number of available logical CPUs. An aggressive CPU limit such as 250m can starve the GC, cause thread pool starvation under load, and create latency spikes, so I would set CPU requests conservatively and limits generously, or omit the CPU limit if the cluster has sufficient capacity. The environment variable `DOTNET_GCConserveMemory` (0–9) can reduce GC memory retention at the cost of more frequent GC cycles, which is useful in memory-constrained containers. If a Pod is repeatedly OOMKilled, `kubectl describe pod <name>` shows `OOMKilled` in the last state, confirming that memory limits are too low rather than pointing to a memory leak.

---

## Q5. What is a Kubernetes Namespace, and when would you use multiple namespaces in a single cluster?

**Concepts**
- HPA scaling based on CPU utilization as percentage of resource request
- Metrics Server as required HPA dependency
- KEDA for queue-depth and event-driven scaling in .NET microservices
- Scale-up responsiveness vs scale-down cool-down trade-off

**Answer**

Horizontal Pod Autoscaling (HPA) is a Kubernetes controller that automatically adjusts the number of Pod replicas in a Deployment or StatefulSet based on observed metrics. The HPA controller polls the Metrics API periodically and scales the replica count up or down within configured minimum and maximum bounds. By default, HPA scales based on average CPU utilization as a percentage of the CPU request defined in the Pod spec — the request must be set on the Pod for HPA to calculate utilization correctly, since without it HPA has no baseline to compute a percentage from. HPA requires the Metrics Server add-on to be installed in the cluster; it provides CPU and memory metrics from kubelet data, and most managed Kubernetes offerings include or easily enable it. Custom and external metrics can drive HPA via adapters — for example, scaling based on the length of an Azure Service Bus or RabbitMQ queue using KEDA (Kubernetes Event-Driven Autoscaling). KEDA is a popular add-on in .NET microservice deployments because services often scale better on queue depth than on CPU utilization. HPA reacts to sustained load rather than spikes — it waits for the metric to exceed the threshold for a few polling cycles before scaling up, and has a longer cool-down before scaling down to avoid thrashing.

---

## Q6. What is a Pod in Kubernetes, and why is it the smallest deployable unit rather than a container?

_Answer not found._

---

## Q7. What is a Deployment, and what does it manage on your behalf?

_Answer not found._

---

## Q8. What is a ReplicaSet, and what role does it play during a Deployment rollout?

_Answer not found._

---

## Q9. What is the difference between a RollingUpdate and a Recreate deployment strategy?

_Answer not found._

---

## Q10. What is a StatefulSet, and when would you use one instead of a Deployment?

_Answer not found._

---

## Q11. Why does Kubernetes use Services to expose Pods rather than using Pod IP addresses directly?

_Answer not found._

---

## Q12. What is the difference between ClusterIP, NodePort, and LoadBalancer Service types?

_Answer not found._

---

## Q13. What is an Ingress resource, and how does it differ from a Service of type LoadBalancer?

_Answer not found._

---

## Q14. How does DNS-based service discovery work for inter-service communication within a cluster?

_Answer not found._

---

## Q15. What is a ConfigMap, and how do you inject configuration from one into a Pod?

_Answer not found._

---

## Q16. What is a Kubernetes Secret, and how does it differ from a ConfigMap in terms of security?

_Answer not found._

---

## Q17. What is the difference between a PersistentVolume and a PersistentVolumeClaim?

_Answer not found._

---

## Q18. When does a .NET microservice need a PersistentVolume, and when can it avoid one?

_Answer not found._

---

## Q19. What is the difference between a liveness probe and a readiness probe in Kubernetes?

_Answer not found._

---

## Q20. How do you configure liveness and readiness probes for an ASP.NET Core application?

_Answer not found._

---

## Q21. What is a startup probe, and when should you add one alongside a liveness probe?

_Answer not found._

---

## Q22. What are resource requests and limits, and how do they affect .NET container behavior?

_Answer not found._

---

## Q23. What is Horizontal Pod Autoscaling, and what metrics does it use by default?

_Answer not found._

---

## Q24. How do you write a multi-stage Dockerfile for an ASP.NET Core application suitable for Kubernetes?

_Answer not found._

---

## Q25. How does graceful shutdown work in a .NET application running inside a Kubernetes Pod?

_Answer not found._

---

## Q26. What environment variable naming convention must you follow to expose hierarchical ASP.NET Core configuration from a Kubernetes ConfigMap or Secret?

_Answer not found._

---

## Q27. What is Helm, and why is it used instead of raw YAML manifests for production Kubernetes deployments?

_Answer not found._

---

## Q28. How does a Kubernetes rolling deployment achieve zero-downtime updates, and what role does the readiness probe play?

_Answer not found._

---

## Q29. What is a PodDisruptionBudget, and why should production deployments define one?

_Answer not found._

---

## Gotchas — Kubernetes Orchestration (Interview Traps)

---

#### Gotcha 1. Confusing Liveness and Readiness Probes

**Concepts**
- Liveness probe restarting a stuck container
- Readiness probe removing a not-yet-ready container from Service endpoints
- Wrong probe type causing restart loops or premature traffic
- Startup probe as a third option for slow-starting applications

**Answer**

A liveness probe that returns failure during normal startup causes Kubernetes to restart the pod in an infinite loop before the application is ready to serve traffic — the container never gets a chance to complete initialisation. A readiness probe misconfigured as a liveness probe means a temporarily unhealthy pod (e.g., during a cache warm-up) gets killed and restarted rather than simply removed from the load balancer pool. The distinction is: liveness tells Kubernetes whether the container needs to be restarted (process is deadlocked); readiness tells Kubernetes whether the container should receive traffic (is it initialised and able to handle requests). For slow-starting applications, a `startupProbe` with a generous `failureThreshold` prevents the liveness probe from killing the pod before the app has started.

---

#### Gotcha 2. No Resource Requests and Limits Defined

**Concepts**
- Scheduler cannot place pods without resource requests
- Noisy-neighbour consuming all node CPU without limits
- OOMKilled pod without memory limits specified
- QoS class (Guaranteed, Burstable, BestEffort) determined by requests/limits

**Answer**

A pod without `resources.requests` defined allows the Kubernetes scheduler to place it on any node regardless of available capacity, potentially overcommitting the node and causing CPU throttling or OOMKilled evictions for all pods on that node. Without `resources.limits`, a memory leak in one pod can consume all node memory and cause the OOM killer to terminate other pods on the same node. Setting requests (the minimum guaranteed allocation used for scheduling) and limits (the maximum allowed) is mandatory for production workloads; the QoS class is `Guaranteed` (requests == limits) for critical latency-sensitive services and `Burstable` (requests < limits) for services that can tolerate occasional throttling.

---

#### Gotcha 3. Rolling Update Disrupting In-Flight Requests

**Concepts**
- Rolling update terminating pods while they serve active HTTP connections
- preStop hook adding a delay before SIGTERM processing
- terminationGracePeriodSeconds giving the app time to drain
- SIGTERM handling in ASP.NET Core with graceful shutdown

**Answer**

During a rolling update, Kubernetes terminates old pods while routing new requests to new pods — but there is a propagation delay between the pod being removed from the Service endpoint list and clients and load balancers actually stopping sending requests to it. Without a `preStop: exec: command: ["/bin/sh", "-c", "sleep 5"]` hook and an adequate `terminationGracePeriodSeconds`, the pod receives SIGTERM and starts shutting down while in-flight requests are still being processed, causing connection-reset errors visible to clients. ASP.NET Core's graceful shutdown drains active connections when `IHostApplicationLifetime.StopApplication()` is called, but Kubernetes must give the pod enough time to drain, set via `terminationGracePeriodSeconds` matching or exceeding the longest expected request duration.

---

#### Gotcha 4. Secrets Stored in ConfigMap Instead of Secret

**Concepts**
- ConfigMap storing plaintext values readable by any pod in the namespace
- Kubernetes Secret base64-encoded but not encrypted by default
- etcd encryption at rest required for true secret security
- External secrets operator integrating with Vault or Key Vault

**Answer**

A Kubernetes `ConfigMap` is intended for non-sensitive configuration — its values are stored as plain text in etcd and readable by any pod in the same namespace with `kubectl get configmap`. Using a ConfigMap for database passwords, API keys, or certificates means any compromised pod or developer with namespace read access can extract them. Kubernetes `Secret` objects are base64-encoded (not encrypted) but have additional access controls and can be encrypted at rest in etcd with the `EncryptionConfiguration` API server feature. For production security, integrate with an external secrets manager — HashiCorp Vault, Azure Key Vault via the External Secrets Operator — which stores the actual secret value outside the cluster and injects it at pod runtime.

---

#### Gotcha 5. Single Replica Deployment With No Pod Disruption Budget

**Concepts**
- Single replica having zero availability during rolling updates
- PodDisruptionBudget preventing forced eviction below minimum available
- minAvailable and maxUnavailable PDB settings
- Node maintenance draining pods versus PDB eviction rejection

**Answer**

A Deployment with `replicas: 1` and no `PodDisruptionBudget` will have zero running pods during a rolling update (the old pod is terminated before the new one is ready) and can have the single pod evicted during node maintenance without warning — the service is completely unavailable. A PodDisruptionBudget with `minAvailable: 1` ensures Kubernetes will not voluntarily evict or drain the last pod, which forces the cluster operator to be intentional about maintenance windows. For services that must have zero planned downtime, `replicas: 2` with `maxUnavailable: 0` in the rolling update strategy and `minAvailable: 1` in the PDB provides continuous availability during both updates and node drains.

---

#### Gotcha 6. Logs Written to Files Instead of stdout/stderr

**Concepts**
- Kubernetes log aggregation reading stdout/stderr from kubelet
- Log file inside container inaccessible without exec or sidecar
- Serilog/NLog configured to write to Console sink in containers
- kubectl logs --follow reading the stdout stream

**Answer**

A container that writes application logs to a file inside the container filesystem (`/app/logs/app.log`) makes those logs inaccessible via `kubectl logs`, invisible to the cluster's log aggregation agents (Fluentd, Fluent Bit, Loki promtail), and lost when the pod is evicted and the container filesystem is destroyed. Containers must write logs to stdout/stderr so the kubelet captures them and log aggregators can read the node's container log files. In ASP.NET Core with Serilog, the production `appsettings.json` must configure the `Console` sink — not a file sink — and the logging format should be JSON structured output for machine parsing by the log aggregator.

---

#### Gotcha 7. imagePullPolicy: Always in Production Increasing Pod Startup Latency

**Concepts**
- imagePullPolicy: Always contacting the registry on every pod start
- Registry unavailability preventing pod scheduling
- imagePullPolicy: IfNotPresent using cached layer
- Immutable image tags removing the need for Always

**Answer**

Setting `imagePullPolicy: Always` causes Kubernetes to contact the image registry and check for updates every time a pod is scheduled — including during scaling events, node reassignments, and pod restarts triggered by liveness probe failures. Under registry outage or network partitioning, `imagePullPolicy: Always` prevents pod scheduling entirely because Kubernetes cannot confirm the image is current. With immutable image tags (every release gets a unique version tag, never reusing the same tag name), `imagePullPolicy: IfNotPresent` is correct: the tag guarantees identity so cached layers are safe to use, and registry availability is required only for the initial pull, not every subsequent pod start.

---

#### Gotcha 8. Horizontal Pod Autoscaler Without Resource Requests Defined

**Concepts**
- HPA scaling based on CPU utilisation as a percentage of requests
- No resource requests making HPA unable to compute utilisation
- HPA showing "unknown" for current utilisation
- Resource requests as a prerequisite for HPA CPU metrics

**Answer**

A HorizontalPodAutoscaler configured to scale on CPU utilisation computes the target as a percentage of the pod's `resources.requests.cpu` — if no CPU request is defined, the HPA cannot calculate a utilisation percentage and will show `<unknown>` as the current value, never scaling. The HPA controller requires `resources.requests.cpu` to be set on the container spec before CPU-based autoscaling can function. Deploying an HPA without resource requests produces a deployment that looks configured for autoscaling but never scales regardless of load, a mistake that is only discovered under the first traffic spike.

---

#### Gotcha 9. Not Handling SIGTERM in the Application

**Concepts**
- Kubernetes sending SIGTERM before SIGKILL
- Application ignoring SIGTERM and being force-killed after terminationGracePeriodSeconds
- ASP.NET Core IHostApplicationLifetime.ApplicationStopping
- In-flight request draining requiring SIGTERM handling

**Answer**

Kubernetes always sends SIGTERM first, waits for `terminationGracePeriodSeconds` (default 30 seconds), then sends SIGKILL if the process has not exited. An application that does not handle SIGTERM — a console app started with `Environment.Exit(0)` on a different signal, or a .NET process ignoring the signal — will be force-killed after 30 seconds with any in-flight requests abruptly terminated and any pending background work abandoned. ASP.NET Core's `IHost` handles SIGTERM via `StopAsync`, which triggers the `ApplicationStopping` cancellation token and drains Kestrel connections, but only if the application uses `Host.CreateDefaultBuilder` or `WebApplication.CreateBuilder` and does not override the default signal handling.

---

#### Gotcha 10. Using NodePort as the Service Type in Production

**Concepts**
- NodePort exposing a fixed port on every cluster node
- Direct node IP traffic bypassing load balancer health checks
- LoadBalancer or Ingress as the correct production exposure pattern
- NodePort used for local clusters only

**Answer**

NodePort services expose a fixed port (30000–32767) on every cluster node's IP address and require clients to know a node IP and the assigned port — there is no automatic load balancing across nodes, no TLS termination, no path-based routing, and no cloud load balancer health integration. A node that goes down leaves clients pointing to a dead IP until they switch to another node IP manually. In production, internet-facing services use a `LoadBalancer` service type (provisioning a cloud load balancer) or a Kubernetes Ingress with a managed Ingress controller (NGINX, Traefik, Azure Application Gateway) for TLS termination, path-based routing, and automatic backend health management. NodePort is appropriate for local development clusters (minikube, kind) where a cloud load balancer is unavailable.

---
