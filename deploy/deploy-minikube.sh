deploy()
{
    # Start minikube and make sure that there is enought resources for istio
    minikube start --cpus 6 --memory 8192

    # Deploy RabbitMq

    # Build images into minikube
    eval $(minikube docker-env)
    sh deploy/deploy-common.sh build

    # Deploy Kubernetes services
    sh deploy/deploy-common.sh deploy_services

    # Open port 8080 to access istio gateway
    kubectl port-forward service/istio-ingressgateway -n istio-system 8080:80
}

destroy()
{
    minikube delete
}

"$@"