# Projeto .NET com Autenticação via AWS Cognito

## Descrição
Este projeto é uma aplicação .NET que utiliza o AWS Cognito para autenticação e gerenciamento de usuários. Para executar a aplicação corretamente, é necessário configurar um pool de usuários no AWS Cognito e fornecer as credenciais apropriadas no `appsettings.json`.

## Configuração do AWS Cognito
Para utilizar o Cognito, siga os passos abaixo:

1. Acesse o [AWS Cognito](https://aws.amazon.com/cognito/)
2. Crie um **User Pool**
3. Configure um **App Client** dentro do User Pool (sem segredo de cliente se for usar fluxo implícito)
4. Anote os seguintes valores:
   - **UserPoolId**
   - **ClientId**
   - **Region**

## Configuração do AWS IAM
Para permitir que sua aplicação acesse o Cognito, você precisa criar um usuário no IAM:

1. Acesse o [AWS IAM](https://aws.amazon.com/iam/)
2. Crie um **novo usuário** com permissões para acessar o Cognito
3. Anexe a política gerenciada `AmazonCognitoPowerUser` ou crie uma personalizada
4. Gere e anote as credenciais de acesso (**Access Key ID** e **Secret Access Key**)

## Configuração do `appsettings.json`
Depois de criar o Cognito, atualize o `appsettings.json` com as informações:

```json
{
 "AWS": {
   "Region": "",
   "UserPoolId": "",
   "AppClientId": "",
   "AppClientSecret": "",
   "CognitoUrl": "",
   "AccessKey": "",
   "AccessSecret": ""
 }
}
```

## Execução do Projeto
1. Instale as dependências:
   ```sh
   dotnet restore
   ```
2. Execute a aplicação:
   ```sh
   dotnet run
   ```

## Testando a Autenticação
Para testar a autenticação, siga os passos abaixo:

### 1. Criar um Usuário
Envie uma requisição POST para o endpoint `/createUser` com os seguintes dados:
```json
{
  "email": "usuario@example.com",
  "password": "SuaSenhaForte123"
}
```

### 2. Confirmar o Cadastro do Usuário
Após a criação do usuário, um código de verificação será enviado para o e-mail informado. Confirme o cadastro enviando um POST para o endpoint `/confirmSignUp` com os seguintes dados:
```json
{
  "email": "usuario@example.com",
  "confirmationCode": "codigo-enviado"
}
```

### 3. Realizar o Login
Agora, você pode testar o login enviando um POST para o endpoint `/login` com as credenciais do usuário cadastrado no Cognito:
```json
{
  "email": "usuario@example.com",
  "password": "SuaSenhaForte123"
}
```

A resposta deve conter um token JWT caso a autenticação seja bem-sucedida.

## Tecnologias Utilizadas
- .NET 8
- AWS Cognito
- AWS IAM
- JWT para autenticação
- ASP.NET Core Identity

