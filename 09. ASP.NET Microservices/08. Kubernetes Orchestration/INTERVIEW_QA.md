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

What is Helm, and why is it used instead of raw YAML manifests for production Kubernetes deployments?

**Answer:** Helm is the package manager for Kubernetes. It bundles related Kubernetes resource definitions (Deployment, Service, ConfigMap, Ingress, etc.) into a reusable, versioned package called a chart, parameterized with a values file so the same chart can be deployed to different environments by overriding a small set of variables. Without Helm, teams maintain separate copies of nearly identical YAML files for each environment, which drifts and becomes inconsistent over time.

- A Helm chart contains Go-templated YAML files in a `templates/` directory, a `Chart.yaml` with metadata and version, and a `values.yaml` with default parameter values. Environment overrides are applied via `-f prod-values.yaml` at install time.
- `helm upgrade --install <release> <chart>` is idempotent: it installs on first run and upgrades on subsequent runs, which makes it safe to run from a CI/CD pipeline on every push.
- `helm rollback <release> <revision>` rolls the deployment back to a previous Helm release revision, which wraps `kubectl rollout undo` with full Helm history tracking.
- Kustomize is a common alternative that uses overlays and patches without templating — it is built into `kubectl apply -k` and preferred when templating complexity is not needed. Many teams use both: Helm for packaging third-party software (cert-manager, nginx Ingress) and Kustomize for their own application configuration.

---

## Q2. What are the main components of the Kubernetes control plane, and what does each one do?

How does a Kubernetes rolling deployment achieve zero-downtime updates, and what role does the readiness probe play?

**Answer:** A rolling update replaces old Pods with new ones incrementally, bounded by the `maxSurge` and `maxUnavailable` settings. Zero-downtime is achieved when `maxUnavailable` is set to zero and the readiness probe is correctly implemented, because Kubernetes only routes traffic to a new Pod after its readiness probe passes, and only terminates an old Pod after the new one is ready.

- With `maxUnavailable: 0` and `maxSurge: 1`, Kubernetes starts one new Pod, waits for its readiness probe to pass, then removes one old Pod — and so on until all replicas are updated. At no point does the number of ready Pods fall below the desired replica count.
- If the new image fails its readiness probe (because the application has a startup bug), the rollout stalls and the old Pods remain running and receiving traffic. The rollout does not proceed to remove the old Pods, which is an automatic safety gate.
- A `preStop` lifecycle hook that sleeps for a few seconds (e.g., 5–15 seconds) on old Pods before they receive SIGTERM gives the Service endpoint list time to propagate the Pod's removal, preventing connection errors from clients that still hold stale references to the old Pod IP.
- PodDisruptionBudgets (PDBs) complement this by preventing more Pods than `maxUnavailable` allows from being voluntarily disrupted simultaneously during events like node drains.

---

## Q3. What runs on each worker node in a Kubernetes cluster?

What is a PodDisruptionBudget, and why should production deployments define one?

**Answer:** A PodDisruptionBudget (PDB) is a Kubernetes policy that limits the number of Pods belonging to a workload that can be voluntarily disrupted at the same time. Voluntary disruptions include node drains (for cluster upgrades, maintenance, or scaling down), evictions for resource pressure, and disruptions triggered by cluster autoscalers. Without a PDB, a cluster upgrade could evict all Pods of a service simultaneously, causing an outage.

- A PDB is expressed either as `minAvailable` (minimum number of Pods that must remain up) or `maxUnavailable` (maximum number that can be disrupted simultaneously). For a service with three replicas and `minAvailable: 2`, at most one Pod can be disrupted at a time.
- PDBs only protect against voluntary disruptions — they cannot prevent a node from failing (involuntary). If a node crashes, Kubernetes reschedules the Pods regardless of the PDB.
- A PDB of `minAvailable: 1` is a common baseline: it ensures the service stays online during node drains while still allowing the cluster to proceed with maintenance. Setting `minAvailable` equal to the replica count blocks all disruptions and will stall cluster upgrades indefinitely.
- PDBs work in conjunction with rolling update settings but they are independent: rolling update `maxUnavailable` controls updates triggered by Deployment changes, while the PDB controls disruptions triggered externally by the cluster control plane.

---

## Q4. What is kubectl, and how does it communicate with the cluster?

What are resource requests and limits, and how do they affect .NET container behavior?

**Answer:** A resource request is the minimum CPU and memory the container is guaranteed to receive; the Kubernetes scheduler uses requests to decide which node can host a Pod. A resource limit is the maximum the container may consume; exceeding the memory limit causes the container to be killed (OOMKilled) and restarted, while exceeding the CPU limit results in throttling (slower execution, not a restart).

- For .NET applications, memory sizing must account for: the .NET runtime itself, the managed heap, native memory used by the GC and thread stack, and any third-party libraries that allocate native memory. A common starting point is 256 Mi request and 512 Mi limit for a small API, monitored and tuned from real load data.
- CPU limits for .NET containers require care. The .NET thread pool sizes itself based on the number of available logical CPUs. An aggressive CPU limit (e.g., 250m, a quarter of a CPU) can starve the GC, cause thread pool starvation under load, and create latency spikes. Set CPU requests conservatively and limits generously, or omit the CPU limit if the cluster has sufficient capacity.
- The environment variable `DOTNET_GCConserveMemory` (0–9) can reduce GC memory retention at the cost of more frequent GC cycles, which is useful in memory-constrained containers.
- If a Pod is repeatedly OOMKilled, `kubectl describe pod <name>` shows `OOMKilled` in the last state, which confirms memory limits are too low rather than a memory leak.

---

## Q5. What is a Kubernetes Namespace, and when would you use multiple namespaces in a single cluster?

What is Horizontal Pod Autoscaling, and what metrics does it use by default?

**Answer:** Horizontal Pod Autoscaling (HPA) is a Kubernetes controller that automatically adjusts the number of Pod replicas in a Deployment or StatefulSet based on observed metrics. The HPA controller polls the Metrics API periodically and scales the replica count up or down within configured minimum and maximum bounds.

- By default, HPA scales based on average CPU utilization as a percentage of the CPU `request` defined in the Pod spec. The `request` must be set on the Pod for HPA to calculate utilization correctly — without it, HPA has no baseline to compute a percentage from.
- HPA requires the Metrics Server add-on to be installed in the cluster; it provides CPU and memory metrics from kubelet data. Most managed Kubernetes offerings (AKS, EKS, GKE) include or easily enable the Metrics Server.
- Custom and external metrics can drive HPA via adapters: for example, scaling based on the length of an Azure Service Bus or RabbitMQ queue using KEDA (Kubernetes Event-Driven Autoscaling). KEDA is a popular add-on in .NET microservice deployments because services often scale better on queue depth than on CPU utilization.
- HPA reacts to sustained load, not spikes — it waits for the metric to exceed the threshold for a few polling cycles before scaling up, and has a longer cool-down before scaling down to avoid thrashing.

---

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
