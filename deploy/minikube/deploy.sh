deploy()
{
    # Start minikube
	minikube start

	# Enable minikube ingress controller
	minikube addons enable ingress

    # Build images into minikube
    eval $(minikube docker-env)

    # Build services
    sh deploy/build-services.sh

    # Deploy services
    sh deploy/deploy-services.sh
}

destroy()
{
    minikube delete
}

"$@"