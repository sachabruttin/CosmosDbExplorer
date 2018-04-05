# CosmosDBS Explorer

A nice client explorer for Microsoft Cosmos DB.
It mimics the Data Explorer available on the Azure Portal on the desktop.

## Features

- Named connection: give meaningfull name to your connection (DEV, QA, PROD)
- Create/Read/Update/Delete for every Cosmos DB resources
- Execute SQL Query on a Collection
- Import document(s) in JSON format
- Colored JSON Editor for documents, stored procedures, user defined functions and triggers
- Customizable Layout with docking windows

## Installation

Download the lastest version from [releases](https://github.com/sachabruttin/CosmosDbExplorer/releases) and unzip it. Run `CosmosDbExplorer.exe`.

## How to Start

1. Add your account from `File>Add Account`
   1. Label: The display name of your account
   2. Account Endpoint: Copy from the `URI` property from the `Keys` menu on the Azure Portal.
   3. Account Secret: Copy `PRIMARY KEY` or `SECONDARY KEY` from the `Keys` menu on the Azure Portal.
2. Start navigating your connection on the treeview pane.
3. Right click any resource to see possible actions. 

