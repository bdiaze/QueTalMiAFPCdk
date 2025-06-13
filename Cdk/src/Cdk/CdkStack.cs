using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Route53;
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
            string prefixRolesWebServer = System.Environment.GetEnvironmentVariable("PREFIX_ROLES_WEB_SERVER") ?? throw new ArgumentNullException("PREFIX_ROLES_WEB_SERVER");
            string s3bucketArn = System.Environment.GetEnvironmentVariable("S3_BUCKET_ARN") ?? throw new ArgumentNullException("S3_BUCKET_ARN");

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

            // Se crea rol de la subapp que asumirá la instancia web server...
            _ = new Role(this, $"{appName}WebServerRole", new RoleProps {
                RoleName = $"{prefixRolesWebServer}{appName}",
                Description = $"Rol de {appName} para ser asumido por Web Server",
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
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
                            ]
                        })
                    }
                }
            });
        }
    }
}
