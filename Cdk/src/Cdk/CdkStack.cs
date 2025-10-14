using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SecretsManager;
using Amazon.CDK.AWS.SSM;
using Constructs;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using Distribution = Amazon.CDK.AWS.CloudFront.Distribution;

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props) {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
            string domainName = System.Environment.GetEnvironmentVariable("DOMAIN_NAME") ?? throw new ArgumentNullException("DOMAIN_NAME");
            string subdomainName = System.Environment.GetEnvironmentVariable("SUBDOMAIN_NAME") ?? throw new ArgumentNullException("SUBDOMAIN_NAME");
            string ec2Host = System.Environment.GetEnvironmentVariable("EC2_HOST") ?? throw new ArgumentNullException("EC2_HOST");
            string ec2RoleArn = System.Environment.GetEnvironmentVariable("EC2_ROLE_ARN") ?? throw new ArgumentNullException("EC2_ROLE_ARN");
            string prefixRolesWebServer = System.Environment.GetEnvironmentVariable("PREFIX_ROLES_WEB_SERVER") ?? throw new ArgumentNullException("PREFIX_ROLES_WEB_SERVER");
            string developmentUser = System.Environment.GetEnvironmentVariable("DEVELOPMENT_USER") ?? throw new ArgumentException("DEVELOPMENT_USER");

            #region URL Scrapers
            string sPensionesUrlApiBase = System.Environment.GetEnvironmentVariable("SPENSIONES_URL_API_BASE") ?? throw new ArgumentNullException("SPENSIONES_URL_API_BASE");
            string afpModeloUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_MODELO_URL_API_BASE") ?? throw new ArgumentNullException("AFP_MODELO_URL_API_BASE");
            string afpModeloV2UrlApiBase = System.Environment.GetEnvironmentVariable("AFP_MODELO_V2_URL_API_BASE") ?? throw new ArgumentNullException("AFP_MODELO_V2_URL_API_BASE");
            string afpModeloV3UrlApiBase = System.Environment.GetEnvironmentVariable("AFP_MODELO_V3_URL_API_BASE") ?? throw new ArgumentNullException("AFP_MODELO_V3_URL_API_BASE");
            string afpModeloV3Base64Key = System.Environment.GetEnvironmentVariable("AFP_MODELO_V3_BASE64_KEY") ?? throw new ArgumentNullException("AFP_MODELO_V3_BASE64_KEY");
            string afpModeloV3Base64IV = System.Environment.GetEnvironmentVariable("AFP_MODELO_V3_BASE64_IV") ?? throw new ArgumentNullException("AFP_MODELO_V3_BASE64_IV");
            string afpCuprumUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_CUPRUM_URL_API_BASE") ?? throw new ArgumentNullException("AFP_CUPRUM_URL_API_BASE");
            string afpCapitalUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_CAPITAL_URL_API_BASE") ?? throw new ArgumentNullException("AFP_CAPITAL_URL_API_BASE");
            string afpHabitatUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_HABITAT_URL_API_BASE") ?? throw new ArgumentNullException("AFP_HABITAT_URL_API_BASE");
            string afpPlanvitalUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_PLANVITAL_URL_API_BASE") ?? throw new ArgumentNullException("AFP_PLANVITAL_URL_API_BASE");
            string afpProvidaUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_PROVIDA_URL_API_BASE") ?? throw new ArgumentNullException("AFP_PROVIDA_URL_API_BASE");
            string afpUnoUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_UNO_URL_API_BASE") ?? throw new ArgumentNullException("AFP_UNO_URL_API_BASE");
            string valoresUfUrlApiBase = System.Environment.GetEnvironmentVariable("VALORES_UF_URL_API_BASE") ?? throw new ArgumentNullException("VALORES_UF_URL_API_BASE");
            string comisionesUrlApiBase = System.Environment.GetEnvironmentVariable("COMISIONES_URL_API_BASE") ?? throw new ArgumentNullException("COMISIONES_URL_API_BASE");
            string comisionesCavUrlApiBase = System.Environment.GetEnvironmentVariable("COMISIONES_CAV_URL_API_BASE") ?? throw new ArgumentNullException("COMISIONES_CAV_URL_API_BASE");
            #endregion

            string queTalMiAfpExtractionKey = System.Environment.GetEnvironmentVariable("QUETALMIAFP_EXTRACTION_KEY") ?? throw new ArgumentNullException("QUETALMIAFP_EXTRACTION_KEY");
            string queTalMiAfpMaxRetries = System.Environment.GetEnvironmentVariable("QUETALMIAFP_MAX_RETRIES") ?? throw new ArgumentNullException("QUETALMIAFP_MAX_RETRIES");
            string queTalMiAfpMilisegForzarTimeout = System.Environment.GetEnvironmentVariable("QUETALMIAFP_MILISEG_FORZAR_TIMEOUT") ?? throw new ArgumentNullException("QUETALMIAFP_MILISEG_FORZAR_TIMEOUT");

            #region Mercado Pago
            string mercadoPagoPublicKey = System.Environment.GetEnvironmentVariable("MERCADOPAGO_PUBLIC_KEY") ?? throw new ArgumentNullException("MERCADOPAGO_PUBLIC_KEY");
            string mercadoPagoAccessToken = System.Environment.GetEnvironmentVariable("MERCADOPAGO_ACCESS_TOKEN") ?? throw new ArgumentNullException("MERCADOPAGO_ACCESS_TOKEN");
            string mercadoPagoUrlSuccess = System.Environment.GetEnvironmentVariable("MERCADOPAGO_URL_SUCCESS") ?? throw new ArgumentNullException("MERCADOPAGO_URL_SUCCESS");
            string mercadoPagoUrlFailure = System.Environment.GetEnvironmentVariable("MERCADOPAGO_URL_FAILURE") ?? throw new ArgumentNullException("MERCADOPAGO_URL_FAILURE");
            string mercadoPagoUrlPending = System.Environment.GetEnvironmentVariable("MERCADOPAGO_URL_PENDING") ?? throw new ArgumentNullException("MERCADOPAGO_URL_PENDING");
            #endregion

            #region Google Recaptcha
            string googleRecaptchaClientKey = System.Environment.GetEnvironmentVariable("GOOGLE_RECAPTCHA_CLIENT_KEY") ?? throw new ArgumentNullException("GOOGLE_RECAPTCHA_CLIENT_KEY");
            string googleRecaptchaSecretKey = System.Environment.GetEnvironmentVariable("GOOGLE_RECAPTCHA_SECRET_KEY") ?? throw new ArgumentNullException("GOOGLE_RECAPTCHA_SECRET_KEY");
            #endregion

            string emailDireccionNotificacion = System.Environment.GetEnvironmentVariable("EMAIL_DIRECCION_NOTIFICACION") ?? throw new ArgumentNullException("EMAIL_DIRECCION_NOTIFICACION");
            string emailNombreNotificacion = System.Environment.GetEnvironmentVariable("EMAIL_NOMBRE_NOTIFICACION") ?? throw new ArgumentNullException("EMAIL_NOMBRE_NOTIFICACION");

            #region Cognito
            string arnParameterUserPoolId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_USER_POOL_ID") ?? throw new ArgumentNullException("ARN_PARAMETER_USER_POOL_ID");
            string arnParameterUserPoolClientId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_USER_POOL_CLIENT_ID") ?? throw new ArgumentNullException("ARN_PARAMETER_USER_POOL_CLIENT_ID");
            string arnParameterCognitoRegion = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_COGNITO_REGION") ?? throw new ArgumentNullException("ARN_PARAMETER_COGNITO_REGION");
            string arnParameterCognitoBaseUrl = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_COGNITO_BASE_URL") ?? throw new ArgumentNullException("ARN_PARAMETER_COGNITO_BASE_URL");
            string arnParameterCognitoCallbacks = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_COGNITO_CALLBACKS") ?? throw new ArgumentNullException("ARN_PARAMETER_COGNITO_CALLBACKS");
            string arnParameterCognitoLogouts = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_COGNITO_LOGOUTS") ?? throw new ArgumentNullException("ARN_PARAMETER_COGNITO_LOGOUTS");
            #endregion 

            string arnParameterSesDireccionDeDefecto = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_SES_DIRECCION_DE_DEFECTO") ?? throw new ArgumentNullException("ARN_PARAMETER_SES_DIRECCION_DE_DEFECTO");

            #region Hermes
            string arnParameterHermesApiUrl = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_HERMES_API_URL") ?? throw new ArgumentNullException("ARN_PARAMETER_HERMES_API_URL");
            string arnParameterHermesApiKeyId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_HERMES_API_KEY_ID") ?? throw new ArgumentNullException("ARN_PARAMETER_HERMES_API_KEY_ID");
            #endregion

            #region API QueTalMiAFP
            string arnParameterQueTalMiAFPApiUrl = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_QUETALMIAFP_API_URL") ?? throw new ArgumentNullException("ARN_PARAMETER_QUETALMIAFP_API_URL");
            string arnParameterQueTalMiAFPApiKeyId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_QUETALMIAFP_API_KEY_ID") ?? throw new ArgumentNullException("ARN_PARAMETER_QUETALMIAFP_API_KEY_ID");
            string arnParameterQueTalMiAFPApiBucketArn = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_QUETALMIAFP_API_BUCKET_ARN") ?? throw new ArgumentNullException("ARN_PARAMETER_QUETALMIAFP_API_BUCKET_ARN");
            string arnParameterQueTalMiAFPApiBucketName = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_QUETALMIAFP_API_BUCKET_NAME") ?? throw new ArgumentNullException("ARN_PARAMETER_QUETALMIAFP_API_BUCKET_NAME");
            #endregion

            IHostedZone hostedZone = HostedZone.FromLookup(this, $"{appName}WebServerHostedZone", new HostedZoneProviderProps {
                DomainName = domainName
            });

            // Se crea record en hosted zone...
            _ = new CnameRecord(this, $"{appName}WebServerCnameRecord", new CnameRecordProps {
                Comment = $"{appName} Web Server Cname Record",
                Zone = hostedZone,
                RecordName = subdomainName,
                DomainName = ec2Host
            });

            #region String Parameters URL Scrapers
            // Se crean todos los parámetros de URL para la extracción de valores...
            StringParameter strParSPensionesUrlApiBase = new(this, $"{appName}StringParameterSPensionesUrlApiBase", new StringParameterProps { 
                ParameterName = $"/{appName}/Extractor/SPensiones/UrlApiBase",
                Description = $"URL API Base para extraccion de valores cuota de SPensiones - {appName}",
                StringValue = sPensionesUrlApiBase,
                Tier = ParameterTier.STANDARD
            });
            StringParameter strParAfpModeloUrlApiBase = new(this, $"{appName}StringParameterAfpModeloUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPModelo/UrlApiBase",
                Description = $"URL API Base para extraccion de AFP Modelo - {appName}",
                StringValue = afpModeloUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpModeloV2UrlApiBase = new(this, $"{appName}StringParameterAfpModeloV2UrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPModeloV2/UrlApiBase",
                Description = $"URL API Base para extraccion de AFP Modelo v2 - {appName}",
                StringValue = afpModeloV2UrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpModeloV3UrlApiBase = new(this, $"{appName}StringParameterAfpModeloV3UrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPModeloV3/UrlApiBase",
                Description = $"URL API Base para extraccion de AFP Modelo v3 - {appName}",
                StringValue = afpModeloV3UrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpModeloV3Base64Key = new(this, $"{appName}StringParameterAfpModeloV3Base64Key", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPModeloV3/Base64Key",
                Description = $"Base64 Key para extraccion de AFP Modelo v3 - {appName}",
                StringValue = afpModeloV3Base64Key,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpModeloV3Base64IV = new(this, $"{appName}StringParameterAfpModeloV3Base64IV", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPModeloV3/Base64IV",
                Description = $"Base64 IV para extraccion de AFP Modelo v3 - {appName}",
                StringValue = afpModeloV3Base64IV,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpCuprumUrlApiBase = new(this, $"{appName}StringParameterAfpCuprumUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPCuprum/UrlApiBase",
                Description = $"URL API Base para extraccion de AFP Cuprum - {appName}",
                StringValue = afpCuprumUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpCapitalUrlApiBase = new(this, $"{appName}StringParameterAfpCapitalUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPCapital/UrlApiBase",
                Description = $"URL API Base para extraccion de AFP Capital - {appName}",
                StringValue = afpCapitalUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpHabitatUrlApiBase = new(this, $"{appName}StringParameterAfpHabitatUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPHabitat/UrlApiBase",
                Description = $"URL API Base para extraccion de AFP Habitat - {appName}",
                StringValue = afpHabitatUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpPlanvitalUrlApiBase = new(this, $"{appName}StringParameterAfpPlanvitalUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPPlanvital/UrlApiBase",
                Description = $"URL API Base para extraccion de AFP Planvital - {appName}",
                StringValue = afpPlanvitalUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpProvidaUrlApiBase = new(this, $"{appName}StringParameterAfpProvidaUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPProvida/UrlApiBase",
                Description = $"URL API Base para extraccion de AFP Provida - {appName}",
                StringValue = afpProvidaUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpUnoUrlApiBase = new(this, $"{appName}StringParameterAfpUnoUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPUno/UrlApiBase",
                Description = $"URL API Base para extraccion de AFP Uno - {appName}",
                StringValue = afpUnoUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParValoresUfUrlApiBase = new(this, $"{appName}StringParameterValoresUfUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/ValoresUf/UrlApiBase",
                Description = $"URL API Base para extraccion de valores Uf - {appName}",
                StringValue = valoresUfUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParComisionesUrlApiBase = new(this, $"{appName}StringParameterComisionesUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/Comisiones/UrlApiBase",
                Description = $"URL API Base para extraccion de comisiones - {appName}",
                StringValue = comisionesUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParComisionesCavUrlApiBase = new(this, $"{appName}StringParameterComisionesCavUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/ComisionesCav/UrlApiBase",
                Description = $"URL API Base para extraccion de comisiones CAV - {appName}",
                StringValue = comisionesCavUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            #endregion

            // Se crean todos los parámetros de la API de QueTalMiAFP...
            StringParameter strParApiMaxRetries = new(this, $"{appName}StringParameterApiMaxRetries", new StringParameterProps {
                ParameterName = $"/{appName}/Api/MaxRetries",
                Description = $"Max retries de API - {appName}",
                StringValue = queTalMiAfpMaxRetries,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParApiMilisegForzarTimeout = new(this, $"{appName}StringParameterApiMilisegForzarTimeout", new StringParameterProps {
                ParameterName = $"/{appName}/Api/MilisegForzarTimeout",
                Description = $"Milisegundos para forzar timeout de API - {appName}",
                StringValue = queTalMiAfpMilisegForzarTimeout,
                Tier = ParameterTier.STANDARD,
            });

            #region String Parameters Mercado Pago
            // Se crean todos los parámetros de Mercado Pago...
            StringParameter strParMercadoPagoPublicKey = new(this, $"{appName}StringParameterMercadoPagoPublicKey", new StringParameterProps {
                ParameterName = $"/{appName}/MercadoPago/PublicKey",
                Description = $"Public Key de Mercado Pago - {appName}",
                StringValue = mercadoPagoPublicKey,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParMercadoPagoUrlSuccess = new(this, $"{appName}StringParameterMercadoPagoUrlSuccess", new StringParameterProps {
                ParameterName = $"/{appName}/MercadoPago/UrlSuccess",
                Description = $"URL success de Mercado Pago - {appName}",
                StringValue = mercadoPagoUrlSuccess,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParMercadoPagoUrlFailure = new(this, $"{appName}StringParameterMercadoPagoUrlFailure", new StringParameterProps {
                ParameterName = $"/{appName}/MercadoPago/UrlFailure",
                Description = $"URL failure de Mercado Pago - {appName}",
                StringValue = mercadoPagoUrlFailure,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParMercadoPagoUrlPending = new(this, $"{appName}StringParameterMercadoPagoUrlPending", new StringParameterProps {
                ParameterName = $"/{appName}/MercadoPago/UrlPending",
                Description = $"URL pending de Mercado Pago - {appName}",
                StringValue = mercadoPagoUrlPending,
                Tier = ParameterTier.STANDARD,
            });
            #endregion

            // Se crean todos los parámetros de Google Recaptcha...
            StringParameter strParGoogleRecaptchaClientKey = new(this, $"{appName}StringParameterGoogleRecaptchaClientKey", new StringParameterProps {
                ParameterName = $"/{appName}/GoogleRecaptcha/ClientKey",
                Description = $"Client Key de Google Recaptcha - {appName}",
                StringValue = googleRecaptchaClientKey,
                Tier = ParameterTier.STANDARD,
            });

            // Se crean todos los parámetros de Email...
            StringParameter strParEmailDireccionNotif = new(this, $"{appName}StringParameterEmailDireccionNotificacion", new StringParameterProps {
                ParameterName = $"/{appName}/Email/DireccionNotificacion",
                Description = $"Direccion a la que se envia notificacion de email - {appName}",
                StringValue = emailDireccionNotificacion,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParEmailNombreNotif = new(this, $"{appName}StringParameterEmailNombreNotificacion", new StringParameterProps {
                ParameterName = $"/{appName}/Email/NombreNotificacion",
                Description = $"Nombre del receptor de la notificacion de email - {appName}",
                StringValue = emailNombreNotificacion,
                Tier = ParameterTier.STANDARD,
            });

            // Se crea secreto para toda la aplicación...
            Secret secret = new(this, $"{appName}Secret", new SecretProps { 
                SecretName = $"/{appName}",
                Description = $"Secretos de la aplicacion {appName}",
                SecretObjectValue = new Dictionary<string, SecretValue> {
                    { "ExtractorKey", SecretValue.UnsafePlainText(queTalMiAfpExtractionKey) },
                    { "MercadoPagoAccessToken", SecretValue.UnsafePlainText(mercadoPagoAccessToken) },
                    { "GoogleRecaptchaSecretKey", SecretValue.UnsafePlainText(googleRecaptchaSecretKey) },
                },
            });

            // Se obtiene ARN del API Key...
            IStringParameter strParHermesApiKeyId = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterHermesApiKeyId", arnParameterHermesApiKeyId);
            IStringParameter strParQueTalMiAFPApiKeyId = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterQueTalMiAFPApiKeyId", arnParameterQueTalMiAFPApiKeyId);

            // Se obtiene ARN del bucket S3 de la API...
            IStringParameter strParQueTalMiAFPApiBucketArn = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterQueTalMiAFPApiBucketArn", arnParameterQueTalMiAFPApiBucketArn);
                        
            // Se crea rol de la subapp que asumirá la instancia web server...
            Role assumeRole = new(this, $"{appName}WebServerRole", new RoleProps {
                RoleName = $"{prefixRolesWebServer}{appName}",
                Description = $"Rol de {appName} para ser asumido por Web Server",
                AssumedBy = new CompositePrincipal(
                    new ArnPrincipal(ec2RoleArn), 
                    new ArnPrincipal(developmentUser)
                ),
                InlinePolicies = new Dictionary<string, PolicyDocument> {
                    {
                        $"{appName}WebServerPolicy",
                        new PolicyDocument(new PolicyDocumentProps{
                            Statements = [
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToGetObject",
                                    Actions = [
                                        "s3:GetObject"
                                    ],
                                    Resources = [
                                        $"{strParQueTalMiAFPApiBucketArn.StringValue}/*",
                                    ],
                                }),
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToSecretManager",
                                    Actions = [
                                        "secretsmanager:GetSecretValue"
                                    ],
                                    Resources = [
                                        secret.SecretFullArn,
                                    ],
                                }),
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToParameterStore",
                                    Actions = [
                                        "ssm:GetParameter"
                                    ],
                                    Resources = [
                                        strParSPensionesUrlApiBase.ParameterArn,
                                        strParAfpCapitalUrlApiBase.ParameterArn,
                                        strParAfpCuprumUrlApiBase.ParameterArn,
                                        strParAfpHabitatUrlApiBase.ParameterArn,
                                        strParAfpModeloUrlApiBase.ParameterArn,
                                        strParAfpModeloV2UrlApiBase.ParameterArn,
                                        strParAfpModeloV3Base64IV.ParameterArn,
                                        strParAfpModeloV3Base64Key.ParameterArn,
                                        strParAfpModeloV3UrlApiBase.ParameterArn,
                                        strParAfpPlanvitalUrlApiBase.ParameterArn,
                                        strParAfpProvidaUrlApiBase.ParameterArn,
                                        strParAfpUnoUrlApiBase.ParameterArn,
                                        strParValoresUfUrlApiBase.ParameterArn,
                                        strParComisionesUrlApiBase.ParameterArn,
                                        strParComisionesCavUrlApiBase.ParameterArn,
                                        strParApiMaxRetries.ParameterArn,
                                        strParApiMilisegForzarTimeout.ParameterArn,
                                        strParEmailDireccionNotif.ParameterArn,
                                        strParEmailNombreNotif.ParameterArn,
                                        strParGoogleRecaptchaClientKey.ParameterArn,
                                        strParMercadoPagoPublicKey.ParameterArn,
                                        strParMercadoPagoUrlFailure.ParameterArn,
                                        strParMercadoPagoUrlPending.ParameterArn,
                                        strParMercadoPagoUrlSuccess.ParameterArn,
                                        arnParameterUserPoolId,
                                        arnParameterUserPoolClientId,
                                        arnParameterCognitoRegion,
                                        arnParameterCognitoBaseUrl,
                                        arnParameterCognitoCallbacks,
                                        arnParameterCognitoLogouts,
                                        arnParameterSesDireccionDeDefecto,
                                        arnParameterHermesApiUrl,
                                        arnParameterHermesApiKeyId,
                                        arnParameterQueTalMiAFPApiUrl,
                                        arnParameterQueTalMiAFPApiKeyId,
                                        arnParameterQueTalMiAFPApiBucketName,
                                    ],
                                }),
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToApiKey",
                                    Actions = [
                                        "apigateway:GET"
                                    ],
                                    Resources = [
                                        $"arn:aws:apigateway:{this.Region}::/apikeys/{strParHermesApiKeyId.StringValue}",
                                        $"arn:aws:apigateway:{this.Region}::/apikeys/{strParQueTalMiAFPApiKeyId.StringValue}",
                                    ],
                                }),
                            ]
                        })
                    }
                }
            });

            StringParameter strParAssumeRoleArn = new(this, $"{appName}StringParameterApiAssumeRoleArn", new StringParameterProps {
                ParameterName = $"/{appName}/Api/AssumeRoleArn",
                Description = $"Assume role ARN - {appName}",
                StringValue = assumeRole.RoleArn,
                Tier = ParameterTier.STANDARD,
            });

            Policy policyAssumeRole = new(this, $"{appName}PolicyAssumeRole", new PolicyProps { 
                PolicyName = $"{appName}PolicyGetParameterAssumeRoleArn",
                Statements = [
                    new PolicyStatement(new PolicyStatementProps{
                        Sid = $"{appName}AccessToParameterStore",
                        Actions = [
                            "ssm:GetParameter"
                        ],
                        Resources = [
                            strParAssumeRoleArn.ParameterArn
                        ],
                    }),
                ]
            });
            assumeRole.AttachInlinePolicy(policyAssumeRole);
        }
    }
}
