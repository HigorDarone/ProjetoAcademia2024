
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System.Configuration;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Services.Description;

namespace ClassLibrary1
{
    public class Class1 : IPlugin
    {
        private IPluginExecutionContext context { get; set; }
        private IOrganizationServiceFactory serviceFactory { get; set; }
        private IOrganizationService serviceUsuario { get; set; }
        private IOrganizationService serviceGlobal { get; set; }
        private ITracingService tracing { get; set; }
        public void Execute(IServiceProvider serviceProvider)
        {
            #region "Cabeçalho essenciais para o plugin"            
            //https://docs.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.ipluginexecutioncontext?view=dynamics-general-ce-9
            //Contexto de execução
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            //Fabrica de conexões
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            //Service no contexto do usuário
            serviceUsuario = serviceFactory.CreateOrganizationService(context.UserId);
            //Service no contexto Global (usuário System)
            serviceGlobal = serviceFactory.CreateOrganizationService(null);
            //Trancing utilizado para reastreamento de mensagem durante o processo
            tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            #endregion

            #region "Verificador de profundidade para evitar loop"
            if (context.Depth > 1) return;
            #endregion


            if (context.MessageName.ToLower().Trim() == "create")
            {
                if (!context.InputParameters.Contains("Target") && !(context.InputParameters["Target"] is Entity)) return;
                Entity entityContext = context.InputParameters["Target"] as Entity;
                //Executando em pos operacao pois assim vai pegar o registro ja criado
                if (context.Stage == (int)Helper.EventStage.PostOperation && context.Mode == (int)Helper.ExecutionMode.Synchronous)
                {
                    if (entityContext.Contains("exer2024_aviaoid"))
                        this.RegistroEmbarque(entityContext);
                    
                }
            }

        }

        public void RegistroEmbarque(Entity entityContext)
        {


            //Pegando o valor do Registro Criado
            Guid IdAviao = entityContext.GetAttributeValue<Guid>("exer2024_aviaoid");



            //Criando array para passar os valores de 1 a 10
            int[] numbers = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };



            //foreach lendo o array e passando para o switch, cada numero vai sigificar o nome logico do campo do assento na tabela
            foreach (int assentos in numbers)
            {

                //criando opcao para instanciar o valor
                OptionSetValue Assento = new OptionSetValue();

                switch (assentos)
                {
                    case 1:
                        Assento = new OptionSetValue(884480000);
                        break;
                    case 2:
                        Assento = new OptionSetValue(884480001);
                        break;
                    case 3:
                        Assento = new OptionSetValue(884480002);
                        break;
                    case 4:
                        Assento = new OptionSetValue(884480003);
                        break;
                    case 5:
                        Assento = new OptionSetValue(884480004);
                        break;
                    case 6:
                        Assento = new OptionSetValue(884480005);
                        break;
                    case 7:
                        Assento = new OptionSetValue(884480006);
                        break;
                    case 8:
                        Assento = new OptionSetValue(884480007);
                        break;
                    case 9:
                        Assento = new OptionSetValue(884480008);
                        break;
                    case 10:
                        Assento = new OptionSetValue(884480009);
                        break;

                }


                //gerando numeros aleatoriamente de 1 a 3 para dar os valores do portao aleatoriamente 
                Random rand = new Random();
                int NumeroAletorio = rand.Next(1,4);

                OptionSetValue portao = new OptionSetValue();

                switch (NumeroAletorio)
                {
                    case 1:
                        portao = new OptionSetValue(884480000);
                        break;
                    case 2:
                        portao = new OptionSetValue(884480001);
                        break;
                    case 3:
                        portao = new OptionSetValue(884480002);
                        break;

                }


                //acessando a tabela e pegando os valores da modelo e do terminal
                QueryExpression nomeDaTabelaAviao = new QueryExpression("exer2024_aviao");
                nomeDaTabelaAviao.ColumnSet = new ColumnSet("exer2024_modelodoaviao", "exer2024_terminal");

                EntityCollection result = serviceGlobal.RetrieveMultiple(nomeDaTabelaAviao);

                

                foreach (Entity entity in result.Entities) 
                {
                  var ModeloAV = entity.GetAttributeValue<string>("exer2024_modelodoaviao");
                  var Terminal = entity.GetAttributeValue<string>("exer2024_terminal");

                //Criando e passando os dados necessarios para a tabela de embarque

                Entity Embarque = new Entity("exer2024_embarque");
                Embarque["exer2024_name"] = $"Modelo do Aviao: {ModeloAV} No terminal: {Terminal}";
                Embarque["exer2024_portao"] = portao;
                Embarque["exer2024_assento"] = Assento;

                //Passando no embarque o campo que vai receber o valor --> em seguida instanciando EntityReference para fazer a referencia do campo de pesquisa,
                //e assim passando o nome logico da tabela e depois o Id que pegamos
                Embarque["exer2024_aviao"] = new EntityReference("exer2024_aviao", IdAviao);

                serviceUsuario.Create(Embarque);
                }

            }

        }

    }
}