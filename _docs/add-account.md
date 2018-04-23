---
title: Add Account
permalink: /docs/add-account/
---

<span class="label label-success">Done</span>

To connect to a Cosmos DB account, you need to add an account in the application. Open the `File` menu and click `Add Account`

![Add Account]({{ site.baseurl }}/img/connection_editor.PNG){:class="img-responsive center-block" height="80%" width="80%"}

### Label

The display name of your account. You can define an easy to remember name for your application (eg. Dev, Test, Prod).

### Account Endpoint

Copy from the `URI` property from the `Keys` menu on the Azure Portal.

### Account Secret

Copy `PRIMARY KEY` or `SECONDARY KEY` from the `Keys` menu on the Azure Portal.

### Accent Color

Choose a color to easily spot tabs related to the account.

### Use local emulator

Check this option when connection to your local emulator.

<span class="label label-info">Info</span>
This use the [default endpoint and secret](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator#authenticating-requests) defined when installing the emulator.

