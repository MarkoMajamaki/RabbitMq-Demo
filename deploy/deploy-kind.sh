deploy()
{
    # Build images
    sh deploy/deploy-common.sh build

    # Create Kind cluster
    kind create cluster --name istio-demo 

    # Deploy RabbitMq
    istioctl install

    # Load images into cluster
    kind load docker-image istio_demo/service1:v1 --name istio-demo
    kind load docker-image istio_demo/service2:v1 --name istio-demo
    kind load docker-image istio_demo/service3:v1 --name istio-demo

    # Deploy Kubernetes services
    sh deploy/deploy-common.sh deploy_services
    
    # Open port 8080 to access istio gateway
    kubectl port-forward service/istio-ingressgateway -n istio-system 8080:80
}

destroy()
{
    kind delete cluster --name istio-demo
}

"$@"