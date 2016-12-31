# Partner Web Storefront

<h2>Overview</h2>

<p>
A web application that acts as a store front for Microsoft partners and enables them to sell Microsoft offers to their customers.
The application gives partners the following features:
<ol>
<li>Configure the Microsoft offers they would like to sell to their customers. Partners can set the price and append extra details.</li>
<li>Configure the portal branding to reflect their company branding. This includes setting the company name, header icons, etc...</li>
<li> payment. Partners can configure their PayPal pro account which will receive payments from customers.</li>
</ol>

Customers can then use the portal to view the offers available, purchase the quantities they need and pay using a credit card.
Customers can log back in and view their subscriptions, purchase extra seats or renew about to expire subscriptions.
</p>

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)
[![Visualize](http://armviz.io/visualizebutton.png)](http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2FPartnerCenterSamples%2FReseller-Web-Application%2Fmaster%2Fazuredeploy.param.json)

<h2>Running the portal</h2>
Clone the portal and perform the following steps:
<ol>
<li>
Go to Partner Center, Account Settings, App Management and onboard a new Web App. Copy the application ID, application secret
and the partner tenant ID into the following settings in Web.Config:<br/><br/>
<div>
&lt;!-- Enter your partner center onboarded AAD application ID here --&gt;<br/>
&lt;add key="partnerCenter.applicationId" value="" /&gt;<br/><br/>
    
&lt;!-- Enter your partner center onboarded AAD application secret here --><br/>
&lt;add key="partnerCenter.applicationSecret" value="" /&gt;<br/><br/>
    
&lt;!-- Enter your partner center AAD tenant ID here --&gt;<br/>
&lt;add key="partnerCenter.AadTenantId" value="" /&gt;<br/><br/>
</div>
</li>
<li>
Create a Web application in your Azure AD tenant. The portal will assume the identity of this application. Change the
following settings in Web.Config to your AD application information:<br/><br/>

<div>
    &lt;!-- The AAD client ID of the application running the web portal --&gt;<br/>
    &lt;add key="webPortal.clientId" value="" /&gt;<br/><br/>

    &lt;!-- The AAD client secret of the application running the web portal --&gt;<br/>
    &lt;add key="webPortal.clientSecret" value="" /&gt;<br/><br/>

    &lt;!-- The AAD tenant ID of the application running the web portal --&gt;<br/>
    &lt;add key="webPortal.AadTenantId" value="" /&gt;<br/><br/>
</div>
</li>
<li>
Provision an Azure storage account which will store the portal's assets and information. Copy its connection string to:</br></br>
<div>
    &lt;!-- The Azure storage connection string which will host the web portal's settings and customers repository. --&gt;</br>
    &lt;add key="webPortal.azureStorageConnectionString" value="" /&gt;
</div>
</li>
<li>
Optionally, specify a REDIS cache connection string to improve performance.<br/><br/>
<div>
    &lt;!-- The Azure Redis cache connection string. Empty value will disable caching. --&gt;<br/>
    &lt;add key="webPortal.cacheConnectionString" value="" /&gt;
</div>
</li>
</ol>

<h2>Deployment</h2>
The portal can be deployed from within Partner Center at: <a href="https://partnercenter.microsoft.com/en-us/pcv/webstore/preparedeployment">https://partnercenter.microsoft.com/en-us/pcv/webstore/preparedeployment</a>.
There is also a deployment project included in the solution through which, deployment can be started with the specified inputs.
