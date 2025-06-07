using Amazon.CDK;
using Amazon.CDK.AWS.Route53;
using Constructs;
using System;

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
        }
    }
}
