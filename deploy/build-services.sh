docker build -t rabbitmq_demo/subscriber:v1 -f services/Subscriber/Dockerfile . 
docker build -t rabbitmq_demo/publisher:v1 -f services/Publisher/Dockerfile .
