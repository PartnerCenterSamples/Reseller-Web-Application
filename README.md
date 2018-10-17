# Partner Center Storefront

![Build status](https://dev.azure.com/partnercenter/storefront/_apis/build/status/partner-center-storefront-CI)

## Overview

A web application that acts as a store front for Microsoft partners and enables them to sell Microsoft offers to their customers. The application gives partners the following features:

1. Configure the Microsoft offers they would like to sell to their customers. Partners can set the price and append extra details.
2. Configure the portal branding to reflect their company branding. This includes setting the company name, header icons, etc...
3. Payment. Partners can configure their PayPal pro account which will receive payments from customers.

The store front application currently supports the following languages (French, Spanish, German and Japanese) along with English which serves as the fallback language. The store front uses the partner's default locale to configure the Locale (Currencies, Date formats, Localized offers in the repository) using the Partner Profile from partner center.

Customers can

1. Use the portal to view the offers available, purchase the quantities they need and make a payment from the storefront.
2. Log back in and view their subscriptions, purchase extra seats or renew about to expire subscriptions.
3. View all the subscriptions (whether they have purchased via the Store front or have been managed for them from Partner Center) in the My Account page after they login.

## Deployment

The storefront can be deployed from within Partner Center. You can start this process by clicking [here](https://partnercenter.microsoft.com/en-us/pcv/webstore/preparedeployment).
There is also a deployment project included in the solution through which, deployment can be started with the specified inputs.

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)
[![Visualize](http://armviz.io/visualizebutton.png)](http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2FMicrosoft%2FPartner-Center-Storefront%2Fmaster%2Fazuredeploy.json)

## Build & Deploy on your own

If you are interested to fork and custom build/deploy the store front. We recommend reading [this blog post](https://blogs.msdn.microsoft.com/iwilliams/2016/12/17/reseller-storefront/) by [Isaiah Williams](https://github.com/isaiahwilliams)

Clone the source code and perform the following steps:

1. Go to Partner Center, Account Settings, App Management and onboard a new Web App. Copy the application ID, application secret and the partner tenant ID into the following settings in Web.Config:

    ```xml
    <!-- Enter your Partner Center AAD application ID here -->
    <add key="partnerCenter.applicationId" value="" />

    <!-- Enter your Partner Center AAD application secret here -->
    <add key="partnerCenter.applicationSecret" value="" />

    <!-- Enter your Partner Center AAD tenant ID here -->
    <add key="partnerCenter.AadTenantId" value="" />
    ```

2. Create a Web application in your Azure AD tenant. The portal will assume the identity of this application. Change the following settings in Web.Config to your AD application information:

    ```xml
    <!-- The AAD client ID of the application running the web portal -->
    <add key="webPortal.clientId" value="" />

    <!-- The AAD client secret of the application running the web portal -->
    <add key="webPortal.clientSecret" value="" />

    <!-- The AAD tenant ID of the application running the web portal -->
    <add key="webPortal.AadTenantId" value="" />

    <!-- The AAD client ID of the application running the web portal -->
    <add key="webPortal.clientId" value="" />

    <!-- The AAD client secret of the application running the web portal -->
    <add key="webPortal.clientSecret" value="" />

    <!-- The AAD tenant ID of the application running the web portal -->
    <add key="webPortal.AadTenantId" value="" />
    ```

3. Provision an Azure storage account which will store the portal's assets and information. Copy its connection string to:

    ```xml
    <!-- The Azure storage connection string which will host the web portal's settings and customers repository. -->
    <add key="webPortal.azureStorageConnectionString" value="" />
    ```

4. Optionally, specify a REDIS cache connection string to improve performance.

    ```xml
    <!-- The Azure Redis cache connection string. Empty value will disable caching. -->
    <add key="webPortal.cacheConnectionString" value="" />
    ```
