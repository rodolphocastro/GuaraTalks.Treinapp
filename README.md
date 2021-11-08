# 🆕 GuaraTalks.Treinapp

## ❓ What is GuaraTalks.Treinapp?

GuaraTalks.Treinapp is a solution that aims to simplify concepts such as Microservices and how they operate with one another by using a very simple domain around sports and workouts.

## 💡 GuaraTalks.Treinapp Domains

The domain for this sample is a very simple sports and workouts system. An Admin should be able to register sports and Workouts, Users should be able to list their planned workouts and attend workout sessions.

### Resources and their Operations

| Resource | Operation | Is Async? (Y/N) | Command or Query? (C/Q) |
| -------- | --------- | --------------- | ----------------------- |
| Sport    | Create    | N               | C                       |
| Sport    | Created   | Y               | Q                       |
| Sport    | List      | N               | Q                       |
| Workout  | List      | N               | Q                       |
| Workout  | Book      | N               | C                       |
| Workout  | Booked    | Y               | Q                       |
| Workout  | Begin     | Y               | C                       |
| Workout  | Began     | Y               | Q                       |
| Workout  | Finish    | Y               | C                       |
| Workout  | Finished  | Y               | Q                       |

## ⚡ Getting Started

WIP

## 🔧 Building and Running

WIP

### 🔨 Build the Project

WIP

### ▶ Running and Settings

WIP

## 🤝 Collaborate with GuaraTalks.Treinapp

WIP

## Kubernetes and Service Mesh

GuaraTalks.Treinapp is builded to be deployed in a k8s cluster with Service Mesh.

### pre-requisites
- minikube
- istioctl
- kubectl
- openssl

### Starting minikube cluster

```shell
minikube start
```

### Installing istio control plane

```shell
istioctl install
```

### Deploying the application

```shell
kubectl apply -f servicemesh-manifest.yml
kubectl apply -f treinapp-manifest.yml
```

### Opening a tunnel to the ingress load balaner
To reach the Istio Ingress Gateway load balancer is necessary to open a tunnel in your machine, that will assing a IP address to the load balancer and you will be able to access the exposed service.

In a new terminal run `minikube tunnel`

e.g.
```shell
$ minikube tunnel
Status:	
	machine: minikube
	pid: 42013
	route: 10.96.0.0/12 -> 192.168.49.2
	minikube: Running
	services: [istio-ingressgateway]
    errors: 
		minikube: no errors
		router: no errors
		loadbalancer emulator: no errors
```

### Configuring TLS certificate to secure requests
- Navigate to `/certificates` folder in the project
- Generate a root certificate to be the Certificate Authority (CA) Private Key
```shell
openssl req -x509 -sha256 -nodes -days 365 -newkey rsa:2048 -subj '/O=example Inc./CN=example.com' -keyout treinapp.com.key -out treinapp.com.crt
```
- Generate Secure Self-Signed Server and Client Certificates
```shell
openssl req -out treinapp.example.com.csr -newkey rsa:2048 -nodes -keyout treinapp.example.com.key -subj "/CN=treinapp.example.com/O=treinapp organization"

openssl x509 -req -days 365 -CA treinapp.com.crt -CAkey treinapp.com.key -set_serial 0 -in treinapp.example.com.csr -out treinapp.example.com.crt
```
- Create the secret
```shell
kubectl create -n istio-system secret tls treinapp-credential --key=treinapp.example.com.key --cert=treinapp.example.com.crt
```
- Example of request using the certificate
```shell
export INGRESS_HOST=$(kubectl -n istio-system get service istio-ingressgateway -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
export SECURE_INGRESS_PORT=$(kubectl -n istio-system get service istio-ingressgateway -o jsonpath='{.spec.ports[?(@.name=="https")].port}')

curl -v --resolve "treinapp.example.com:$SECURE_INGRESS_PORT:$INGRESS_HOST" \
--cacert certificates/treinapp.example.com.crt "https://treinapp.example.com:$SECURE_INGRESS_PORT"
```