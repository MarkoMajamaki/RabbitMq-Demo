build()
{
    docker build -t istio_demo/service1:v1 -f services/Service1/Dockerfile . 
    docker build -t istio_demo/service2:v1 -f services/Service2/Dockerfile .
    docker build -t istio_demo/service3:v1 -f services/Service3/Dockerfile .
}

deploy_services()
{
    kubectl apply -f kubernetes/service1.yaml
    kubectl apply -f kubernetes/service2.yaml
    kubectl apply -f kubernetes/service3.yaml
}

"$@"