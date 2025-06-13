using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.SSM;
using Constructs;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
            string domainName = System.Environment.GetEnvironmentVariable("DOMAIN_NAME") ?? throw new ArgumentNullException("DOMAIN_NAME");
            string subdomainName = System.Environment.GetEnvironmentVariable("SUBDOMAIN_NAME") ?? throw new ArgumentNullException("SUBDOMAIN_NAME");
            string ec2Host = System.Environment.GetEnvironmentVariable("EC2_HOST") ?? throw new ArgumentNullException("EC2_HOST");
            string ec2RoleArn = System.Environment.GetEnvironmentVariable("EC2_ROLE_ARN") ?? throw new ArgumentNullException("EC2_ROLE_ARN");
            string prefixRolesWebServer = System.Environment.GetEnvironmentVariable("PREFIX_ROLES_WEB_SERVER") ?? throw new ArgumentNullException("PREFIX_ROLES_WEB_SERVER");
            string s3bucketArn = System.Environment.GetEnvironmentVariable("S3_BUCKET_ARN") ?? throw new ArgumentNullException("S3_BUCKET_ARN");

            string afpModeloUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_MODELO_URL_API_BASE") ?? throw new ArgumentNullException("AFP_MODELO_URL_API_BASE");
            string afpModeloV2UrlApiBase = System.Environment.GetEnvironmentVariable("AFP_MODELO_V2_URL_API_BASE") ?? throw new ArgumentNullException("AFP_MODELO_V2_URL_API_BASE");
            string afpModeloV3UrlApiBase = System.Environment.GetEnvironmentVariable("AFP_MODELO_V3_URL_API_BASE") ?? throw new ArgumentNullException("AFP_MODELO_V3_URL_API_BASE");
            string afpModeloV3Base64Key = System.Environment.GetEnvironmentVariable("AFP_MODELO_V3_BASE64_KEY") ?? throw new ArgumentNullException("AFP_MODELO_V3_BASE64_KEY");
            string afpModeloV3Base64IV = System.Environment.GetEnvironmentVariable("AFP_MODELO_V3_BASE64_IV") ?? throw new ArgumentNullException("AFP_MODELO_V3_BASE64_IV");
            string afpCuprumUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_CUPRUM_URL_API_BASE") ?? throw new ArgumentNullException("AFP_CUPRUM_URL_API_BASE");
            string afpCapitalUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_CAPITAL_URL_API_BASE") ?? throw new ArgumentNullException("AFP_CAPITAL_URL_API_BASE");
            string afpCapitalV2UrlApiBase = System.Environment.GetEnvironmentVariable("AFP_CAPITAL_V2_URL_API_BASE") ?? throw new ArgumentNullException("AFP_CAPITAL_V2_URL_API_BASE");
            string afpHabitatUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_HABITAT_URL_API_BASE") ?? throw new ArgumentNullException("AFP_HABITAT_URL_API_BASE");
            string afpPlanvitalUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_PLANVITAL_URL_API_BASE") ?? throw new ArgumentNullException("AFP_PLANVITAL_URL_API_BASE");
            string afpProvidaUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_PROVIDA_URL_API_BASE") ?? throw new ArgumentNullException("AFP_PROVIDA_URL_API_BASE");
            string afpUnoUrlApiBase = System.Environment.GetEnvironmentVariable("AFP_UNO_URL_API_BASE") ?? throw new ArgumentNullException("AFP_UNO_URL_API_BASE");
            string valoresUfUrlApiBase = System.Environment.GetEnvironmentVariable("VALORES_UF_URL_API_BASE") ?? throw new ArgumentNullException("VALORES_UF_URL_API_BASE");
            string comisionesUrlApiBase = System.Environment.GetEnvironmentVariable("COMISIONES_URL_API_BASE") ?? throw new ArgumentNullException("COMISIONES_URL_API_BASE");
            string comisionesCavUrlApiBase = System.Environment.GetEnvironmentVariable("COMISIONES_CAV_URL_API_BASE") ?? throw new ArgumentNullException("COMISIONES_CAV_URL_API_BASE");
            string queTalMiAfpExtractionKey = System.Environment.GetEnvironmentVariable("QUETALMIAFP_EXTRACTION_KEY") ?? throw new ArgumentNullException("QUETALMIAFP_EXTRACTION_KEY");
            string queTalMiAfpApiUrl = System.Environment.GetEnvironmentVariable("QUETALMIAFP_API_URL") ?? throw new ArgumentNullException("QUETALMIAFP_API_URL");
            string queTalMiAfpApiKey = System.Environment.GetEnvironmentVariable("QUETALMIAFP_API_KEY") ?? throw new ArgumentNullException("QUETALMIAFP_API_KEY");
            string queTalMiAfpS3BucketName = System.Environment.GetEnvironmentVariable("QUETALMIAFP_S3_BUCKET_NAME") ?? throw new ArgumentNullException("QUETALMIAFP_S3_BUCKET_NAME");
            string queTalMiAfpMaxRetries = System.Environment.GetEnvironmentVariable("QUETALMIAFP_MAX_RETRIES") ?? throw new ArgumentNullException("QUETALMIAFP_MAX_RETRIES");
            string queTalMiAfpMilisegForzarTimeout = System.Environment.GetEnvironmentVariable("QUETALMIAFP_MILISEG_FORZAR_TIMEOUT") ?? throw new ArgumentNullException("QUETALMIAFP_MILISEG_FORZAR_TIMEOUT");
            string mercadoPagoPublicKey = System.Environment.GetEnvironmentVariable("MERCADOPAGO_PUBLIC_KEY") ?? throw new ArgumentNullException("MERCADOPAGO_PUBLIC_KEY");
            string mercadoPagoAccessToken = System.Environment.GetEnvironmentVariable("MERCADOPAGO_ACCESS_TOKEN") ?? throw new ArgumentNullException("MERCADOPAGO_ACCESS_TOKEN");
            string mercadoPagoUrlSuccess = System.Environment.GetEnvironmentVariable("MERCADOPAGO_URL_SUCCESS") ?? throw new ArgumentNullException("MERCADOPAGO_URL_SUCCESS");
            string mercadoPagoUrlFailure = System.Environment.GetEnvironmentVariable("MERCADOPAGO_URL_FAILURE") ?? throw new ArgumentNullException("MERCADOPAGO_URL_FAILURE");
            string mercadoPagoUrlPending = System.Environment.GetEnvironmentVariable("MERCADOPAGO_URL_PENDING") ?? throw new ArgumentNullException("MERCADOPAGO_URL_PENDING");
            string googleRecaptchaClientKey = System.Environment.GetEnvironmentVariable("GOOGLE_RECAPTCHA_CLIENT_KEY") ?? throw new ArgumentNullException("GOOGLE_RECAPTCHA_CLIENT_KEY");
            string googleRecaptchaSecretKey = System.Environment.GetEnvironmentVariable("GOOGLE_RECAPTCHA_SECRET_KEY") ?? throw new ArgumentNullException("GOOGLE_RECAPTCHA_SECRET_KEY");
            string gmailDireccion = System.Environment.GetEnvironmentVariable("GMAIL_DIRECCION") ?? throw new ArgumentNullException("GMAIL_DIRECCION");
            string gmailDireccionAlias = System.Environment.GetEnvironmentVariable("GMAIL_DIRECCION_ALIAS") ?? throw new ArgumentNullException("GMAIL_DIRECCION_ALIAS");
            string gmailNombre = System.Environment.GetEnvironmentVariable("GMAIL_NOMBRE") ?? throw new ArgumentNullException("GMAIL_NOMBRE");
            string gmailDireccionNotificacion = System.Environment.GetEnvironmentVariable("GMAIL_DIRECCION_NOTIFICACION") ?? throw new ArgumentNullException("GMAIL_DIRECCION_NOTIFICACION");
            string gmailNombreNotificacion = System.Environment.GetEnvironmentVariable("GMAIL_NOMBRE_NOTIFICACION") ?? throw new ArgumentNullException("GMAIL_NOMBRE_NOTIFICACION");
            string gmailPrivateKey = System.Environment.GetEnvironmentVariable("GMAIL_PRIVATE_KEY") ?? throw new ArgumentNullException("GMAIL_PRIVATE_KEY");
            string gmailClientEmail = System.Environment.GetEnvironmentVariable("GMAIL_CLIENT_EMAIL") ?? throw new ArgumentNullException("GMAIL_CLIENT_EMAIL");

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

            // Se crean todos los parámetros de URL para la extracción de valores...
            StringParameter strParAfpModeloUrlApiBase = new(this, $"{appName}StringParameterAfpModeloUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPModelo/UrlApiBase",
                Description = $"URL API Base para extracción de AFP Modelo - {appName}",
                StringValue = afpModeloUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpModeloV2UrlApiBase = new(this, $"{appName}StringParameterAfpModeloV2UrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPModeloV2/UrlApiBase",
                Description = $"URL API Base para extracción de AFP Modelo v2 - {appName}",
                StringValue = afpModeloV2UrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpModeloV3UrlApiBase = new(this, $"{appName}StringParameterAfpModeloV3UrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPModeloV3/UrlApiBase",
                Description = $"URL API Base para extracción de AFP Modelo v3 - {appName}",
                StringValue = afpModeloV3UrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpModeloV3Base64Key = new(this, $"{appName}StringParameterAfpModeloV3Base64Key", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPModeloV3/Base64Key",
                Description = $"Base64 Key para extracción de AFP Modelo v3 - {appName}",
                StringValue = afpModeloV3Base64Key,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpModeloV3Base64IV = new(this, $"{appName}StringParameterAfpModeloV3Base64IV", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPModeloV3/Base64IV",
                Description = $"Base64 IV para extracción de AFP Modelo v3 - {appName}",
                StringValue = afpModeloV3Base64IV,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpCuprumUrlApiBase = new(this, $"{appName}StringParameterAfpCuprumUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPCuprum/UrlApiBase",
                Description = $"URL API Base para extracción de AFP Cuprum - {appName}",
                StringValue = afpCuprumUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpCapitalUrlApiBase = new(this, $"{appName}StringParameterAfpCapitalUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPCapital/UrlApiBase",
                Description = $"URL API Base para extracción de AFP Capital - {appName}",
                StringValue = afpCapitalUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpCapitalV2UrlApiBase = new(this, $"{appName}StringParameterAfpCapitalV2UrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPCapitalV2/UrlApiBase",
                Description = $"URL API Base para extracción de AFP Capital v2 - {appName}",
                StringValue = afpCapitalV2UrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpHabitatUrlApiBase = new(this, $"{appName}StringParameterAfpHabitatUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPHabitat/UrlApiBase",
                Description = $"URL API Base para extracción de AFP Habitat - {appName}",
                StringValue = afpHabitatUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpPlanvitalUrlApiBase = new(this, $"{appName}StringParameterAfpPlanvitalUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPPlanvital/UrlApiBase",
                Description = $"URL API Base para extracción de AFP Planvital - {appName}",
                StringValue = afpPlanvitalUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpProvidaUrlApiBase = new(this, $"{appName}StringParameterAfpProvidaUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPProvida/UrlApiBase",
                Description = $"URL API Base para extracción de AFP Provida - {appName}",
                StringValue = afpProvidaUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParAfpUnoUrlApiBase = new(this, $"{appName}StringParameterAfpUnoUrlApiBase", new StringParameterProps {
                ParameterName = $"/{appName}/Extractor/AFPUno/UrlApiBase",
                Description = $"URL API Base para extracción de AFP Uno - {appName}",
                StringValue = afpUnoUrlApiBase,
                Tier = ParameterTier.STANDARD,
            });

            // Se crean todos los parámetros de la API de QueTalMiAFP...
            StringParameter strParApiUrl = new(this, $"{appName}StringParameterApiUrl", new StringParameterProps {
                ParameterName = $"/{appName}/Api/Url",
                Description = $"URL de API - {appName}",
                StringValue = queTalMiAfpApiUrl,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParApiS3BucketName = new(this, $"{appName}StringParameterApiS3BucketName", new StringParameterProps {
                ParameterName = $"/{appName}/Api/S3BucketName",
                Description = $"S3 bucket name de API - {appName}",
                StringValue = queTalMiAfpS3BucketName,
                Tier = ParameterTier.STANDARD,
            });
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

            // Se crean todos los parámetros de Google Recaptcha...
            StringParameter strParGoogleRecaptchaClientKey = new(this, $"{appName}StringParameterGoogleRecaptchaClientKey", new StringParameterProps {
                ParameterName = $"/{appName}/GoogleRecaptcha/ClientKey",
                Description = $"Client Key de Google Recaptcha - {appName}",
                StringValue = googleRecaptchaClientKey,
                Tier = ParameterTier.STANDARD,
            });

            // Se crean todos los parámetros de Gmail...
            StringParameter strParGmailDireccion = new(this, $"{appName}StringParameterGmailDireccion", new StringParameterProps {
                ParameterName = $"/{appName}/Gmail/Direccion",
                Description = $"Dirección de Gmail desde donde se mandan las notificaciones - {appName}",
                StringValue = googleRecaptchaClientKey,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParGmailDireccionAlias = new(this, $"{appName}StringParameterGmailDireccionAlias", new StringParameterProps {
                ParameterName = $"/{appName}/Gmail/DireccionAlias",
                Description = $"Alias de la dirección de Gmail desde donde se mandan las notificaciones  - {appName}",
                StringValue = googleRecaptchaClientKey,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParGmailNombre = new(this, $"{appName}StringParameterGmailNombre", new StringParameterProps {
                ParameterName = $"/{appName}/Gmail/Nombre",
                Description = $"Nombre a mostrarse como remitente de la notificación - {appName}",
                StringValue = googleRecaptchaClientKey,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParGmailDireccionNotif = new(this, $"{appName}StringParameterGmailDireccionNotificacion", new StringParameterProps {
                ParameterName = $"/{appName}/Gmail/DireccionNotificacion",
                Description = $"Direccion a la que se envía notificación de Gmail - {appName}",
                StringValue = googleRecaptchaClientKey,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParGmailNombreNotif = new(this, $"{appName}StringParameterGmailNombreNotificacion", new StringParameterProps {
                ParameterName = $"/{appName}/Gmail/NombreNotificacion",
                Description = $"Nombre del receptor de la notificación de Gmail - {appName}",
                StringValue = googleRecaptchaClientKey,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter strParGmailClientEmail = new(this, $"{appName}StringParameterGmailClientEmail", new StringParameterProps {
                ParameterName = $"/{appName}/Gmail/ClientEmail",
                Description = $"Client Email de Gmail - {appName}",
                StringValue = googleRecaptchaClientKey,
                Tier = ParameterTier.STANDARD,
            });

            // Se crea rol de la subapp que asumirá la instancia web server...
            Role assumeRole = new(this, $"{appName}WebServerRole", new RoleProps {
                RoleName = $"{prefixRolesWebServer}{appName}",
                Description = $"Rol de {appName} para ser asumido por Web Server",
                AssumedBy = new ArnPrincipal(ec2RoleArn),
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
                                        $"{s3bucketArn}/*",
                                    ],
                                }),
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToParameterStore",
                                    Actions = [
                                        "ssm:GetParameter"
                                    ],
                                    Resources = [
                                        strParAfpCapitalUrlApiBase.ParameterArn,
                                        strParAfpCapitalV2UrlApiBase.ParameterArn,
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
                                        strParApiMaxRetries.ParameterArn,
                                        strParApiMilisegForzarTimeout.ParameterArn,
                                        strParApiS3BucketName.ParameterArn,
                                        strParApiUrl.ParameterArn,
                                        strParGmailClientEmail.ParameterArn,
                                        strParGmailDireccion.ParameterArn,
                                        strParGmailDireccionAlias.ParameterArn,
                                        strParGmailDireccionNotif.ParameterArn,
                                        strParGmailNombre.ParameterArn,
                                        strParGmailNombreNotif.ParameterArn,
                                        strParGoogleRecaptchaClientKey.ParameterArn,
                                        strParMercadoPagoPublicKey.ParameterArn,
                                        strParMercadoPagoUrlFailure.ParameterArn,
                                        strParMercadoPagoUrlPending.ParameterArn,
                                        strParMercadoPagoUrlSuccess.ParameterArn,
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
