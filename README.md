## Projeto destinado a criar uma App Function para plataforma azure ##
### Introdução ###
Faz parte do escopo deste projeto: criar uma função azure para coletar dados de uma API de cotação de ações "[AlphaVantage](https://www.alphavantage.co/)".
O repositório tem duas funções em diferentes branchs, na master temos uma Trigger Function que recebe o parametro o Simbolo da Stock (código da ação listado na bolsa de valores) e retorna a cotação dos ultimos 30 dias.
enquanto a outra branch temos uma Timer Function que chama a API da Apha a cada 2 minnutos enviando por e-mail a cotação do simbol PETR4.SA dos últimos 30 dias.
### Objetivos ###
* **Integração entre APIs**: Demonstrar a comunicação da Azure Function consumindo e enviando dados para serviços externos.
* **Dois tipos de Azure Functions**: Implementar de forma prática os gatilhos de rota (**HttpTrigger**) e de tempo (**TimerTrigger**).
* **Fluxo de Branches & Funcionalidades**:
  * **Branch Secundária**: Desenvolvimento isolado de tarefas agendadas e tratamento de requisições parametrizadas.
  * **Branch Master**: Centralização do código estável e consolidação da integração principal entre as APIs.
* **Ferramentas de Desenvolvimento Local**: Configurar e utilizar o **Azurite** (emulador de Storage) e o **Azure Functions Core Tools** para testes 100% offline.

### Instalação e Requisitos ###
```bash
# Instala o runtime do Azure Functions localmente
npm i -g azure-functions-core-tools@4

# Instala o emulador de armazenamento do Azure
npm i -g azurite
```
* Criar conta na AlphaVantage - https://www.alphavantage.co e copiar a chaave de api
* Criar conta na RESEND - https://resend.com/ para poder testar a api de emails
Após baixar o código na pasta raiz crie um arquivo json chamado local.settings.json e cole nele o seguinte código
```bash
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "RESEND_API_KEY":"SUA_CHAVE_RESEND_APP",
        "ALPHAVANTAGE_API_KEY":"SUA_CHAVE_APHAVANTAGE_AQUI"
   }
}
```
Este código deve ser colocado no seu arquivo e o valor das variáveis exige a criação de conta na ALPHAVANTAGE (API para coletar os dados da bolsa)e na RESEND(APi para reenvio de emails)
### Publicação ###
partindo do pressuposto que ja existe os recursos e a appfunction na plataforma azure e que ja tem o azure CLI rodando
```bash
# Sobe o projeto criado localmente para a AppFunction ja criada na plataforma Azure
az functionapp config appsettings set --name dotnet-az-function --resource-group DefaultResourceGroup-CQ --settings "ALPHAVANTAGE_API_KEY=XXXXXXXXXXXXXXX"
# Publica o projeto
func azure functionapp publish dotnet-az-function
```
### Ponto de atenção ###
Pode ser necessário usar uma x-functions-key para ter acesso a API. Se for necessario pela plataforma azure acesse: 
Microsoft Entra Id > ID do Locatário e no Header da solicitação cole x-functions-key no nome do parâmetro e no valor cole a sua key
### Curl de teste no PostMan ###
curl --location 'https://dotnet-az-function-evcjhzd6f8ejb7a3.brazilsouth-01.azurewebsites.net/api/DailyAlphaQuote/PETR4.SA' \
--header 'x-functions-key: COLE_SUA_KEY'
