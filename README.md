## .NET + RabbitMQ
O objetivo desse repositório é servir como exemplo para a inicialização de um Broker RabbitMQ usando o .NET.

#### Tecnologias utilizadas 
* .NET 8
* RabbitMQ


## Arquitetura Publish/Subscribe
A arquitetura publish/subscribe (pub/sub) é um padrão de comunicação onde os "publishers" enviam mensagens para um broker de mensageria e os "subscribers" recebem as mensagens com base nas filas aos quais estão inscritos.

#### Fluxo Publish
1. Se conecta ao servidor RabbitMQ
2. Cria um canal de comunicação
3. Publica mensagem em uma exchange

#### Fluxo Subscribe
1. Se conecta ao servidor RabbitMQ
2. Cria um canal de comunicação
3. Define uma fila para aguardar mensagens
4. Confirma ou rejeita mensagens

## Inicialização de Broker RabbitMQ via código .NET
No código apresentado no repositório, no projeto "PubSubRabbitMQ.Publisher" é realizada a inicialização das filas, exchanges e bindings utilizando a biblioteca RabbitMQ.Client


![Imagem arquitetura RabbitMQ](https://github.com/eduardocaas/.NET_RabbitMQ/blob/main/img/Arq_RabbitMQ.png?raw=true)
