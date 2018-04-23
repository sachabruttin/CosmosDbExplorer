---
image: /assets/documentDbColor.png
---

# Cosmos DB Explorer

A nice client explorer for Microsoft Cosmos DB.
It mimics the Data Explorer available on the Azure Portal on the desktop.

## [](#installation)Installation

Download the lastest version from [releases]({{ site.github.repository_url }}/releases) and unzip it. Run `CosmosDbExplorer.exe`.

## [](#how-to-start)How to Start

1. Add your account from `File>Add Account`
   1. Label: The display name of your account
   2. Account Endpoint: Copy from the `URI` property from the `Keys` menu on the Azure Portal.
   3. Account Secret: Copy `PRIMARY KEY` or `SECONDARY KEY` from the `Keys` menu on the Azure Portal.
   4. Choose a color to easily spot tabs related to the account.
2. Start navigating your connection on the treeview pane.
3. Right click any resource to see possible actions. 


## [](#features)Features

### Named connection

Give a meaningfull name to your account connection (e.g. DEV, QA, PROD). Assign a color to easily distinguish between them into the document section.

![Account Settings](/assets/connection_editor.PNG)

### Create/Read/Update/Delete for every Cosmos DB resources

Manage all your resources from the application. 

![Ressources Tree](/assets/ressources.PNG)

### Execute SQL Query Editor

Enjoy syntax highlighting when editing your query.

Input SQL Query and run them in place and navigate into the response document.

Define your query settings:
- Enable Scan In Query
- Enable Cross Partition Query
- Max Items Count
- Max Degree of Parallelism
- Max Buffer

Get the query metrics, response headers and request charge (RU) automatically for each query.
Pagged results is supported!

![Query Editor](/assets/query_editor.PNG)

### Stored Procedure

Edit Stored Procedure and execute them directly from the tool.

Cosmos DB Explorer supports two kind of parameters: JSON or File. With JSON you can enter any kind of valid JSON value (e.g. string, number, object, array). And with File you can set the content of a file as the source of your parameter. 

- Syntax Highlighting for JavaScript code
- JSON or File parameter
- Get detailed result
- Get the HTTP Headers returned by the server
- Use `console.log()` and get debugging information
- Export results to file
- Supports Partitioned Collections

![Stored Procedure](/assets/stored_procedure.PNG)

### Colored JSON Editor

Have a fully colored JSON viewer and editor. Easilly find value into your document.


### Customizable Layout with docking windows

Organize your documents side-by-side or hide information that you don't need.