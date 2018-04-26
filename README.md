# Partner Web Storefront 
![BuildStatus](https://ci.appveyor.com/api/projects/status/github/PartnerCenterSamples/Reseller-Web-Application?branch=master&svg=true)


## Overview

A web application that acts as a store front for Microsoft partners and enables them to sell Microsoft offers to their customers.
The application gives partners the following features:

1. Configure the Microsoft offers they would like to sell to their customers. Partners can set the price and append extra details.
2. Configure the portal branding to reflect their company branding. This includes setting the company name, header icons, etc...
3. Payment. Partners can configure their PayPal pro account which will receive payments from customers

The store front application currently supports the following languages (French, Spanish, German and Japanese) along with English which serves as the fallback language. 
The store front uses the partner's default locale to configure the Locale (Currencies, Date formats, Localized offers in the repository) using the Partner Profile from partner center. 

Customers can 
1. Use the portal to view the offers available, purchase the quantities they need and make a payment from the storefront.
2. Log back in and view their subscriptions, purchase extra seats or renew about to expire subscriptions.
3. View all the subscriptions (whether they have purchased via the Store front or have been managed for them from Partner Center) in the My Account page after they login. 

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Deployment
The portal can be deployed from within Partner Center at: <a href="https://partnercenter.microsoft.com/en-us/pcv/webstore/preparedeployment">https://partnercenter.microsoft.com/en-us/pcv/webstore/preparedeployment</a>.
There is also a deployment project included in the solution through which, deployment can be started with the specified inputs.

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)
[![Visualize](http://armviz.io/visualizebutton.png)](http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2FPartnerCenterSamples%2FReseller-Web-Application%2Fmaster%2Fazuredeploy.json)

## Build & Deploy on your own
If you are interested to fork and custom build/deploy the store front. We recommend reading [this blog post](https://blogs.msdn.microsoft.com/iwilliams/2016/12/17/reseller-storefront/) by [Isaiah Williams](https://github.com/isaiahwilliams)

Clone the source code and perform the following steps:

1. Go to Partner Center, Account Settings, App Management and onboard a new Web App. Copy the application ID, application secret
and the partner tenant ID into the following settings in Web.Config:

    &lt;!-- Enter your partner center onboarded AAD application ID here --&gt; <br/>
    &lt;add key="partnerCenter.applicationId" value="" /&gt;
        
    &lt;!-- Enter your partner center onboarded AAD application secret here --><br/>
    &lt;add key="partnerCenter.applicationSecret" value="" /&gt;
        
    &lt;!-- Enter your partner center AAD tenant ID here --&gt; <br />
    &lt;add key="partnerCenter.AadTenantId" value="" /&gt;

2. Create a Web application in your Azure AD tenant. The portal will assume the identity of this application. Change the
following settings in Web.Config to your AD application information:

    &lt;!-- The AAD client ID of the application running the web portal --&gt;<br/>
    &lt;add key="webPortal.clientId" value="" /&gt;

    &lt;!-- The AAD client secret of the application running the web portal --&gt;<br/>
    &lt;add key="webPortal.clientSecret" value="" /&gt;

    &lt;!-- The AAD tenant ID of the application running the web portal --&gt;<br/>
    &lt;add key="webPortal.AadTenantId" value="" /&gt;

3. Provision an Azure storage account which will store the portal's assets and information. Copy its connection string to:

    &lt;!-- The Azure storage connection string which will host the web portal's settings and customers repository. --&gt;</br>
    &lt;add key="webPortal.azureStorageConnectionString" value="" /&gt;


3. Optionally, specify a REDIS cache connection string to improve performance.

    &lt;!-- The Azure Redis cache connection string. Empty value will disable caching. --&gt;<br/>
    &lt;add key="webPortal.cacheConnectionString" value="" /&gt;
