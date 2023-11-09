# Service Framework

The Service Framework package by the [Reality Collective](https://www.realityCollective.io). This package is an extensible service framework to build highly performant components for your Unity projects.

[![openupm](https://img.shields.io/npm/v/com.realitycollective.service-framework?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.realitycollective.service-framework/)
[![Discord](https://img.shields.io/discord/597064584980987924.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/hF7TtRCFmB)
[![Publish development branch on Merge](https://github.com/realitycollective/com.realitycollective.service-framework/actions/workflows/development-publish.yml/badge.svg)](https://github.com/realitycollective/com.realitycollective.service-framework/actions/workflows/development-publish.yml)
[![Build and test UPM packages for platforms, all branches except main](https://github.com/realitycollective/com.realitycollective.service-framework/actions/workflows/development-buildandtestupmrelease.yml/badge.svg)](https://github.com/realitycollective/com.realitycollective.service-framework/actions/workflows/development-buildandtestupmrelease.yml)

## Overview

The Service framework provides a service repository for enabling background services to run efficiently in a Unity project, it features capabilities such as:

- Platform specific operation - choose which platforms your service runs on.
- Zero Latency from Unity operations - services are fully c# based with no Unity overhead.
- Ability to host several sub-services (service modules) as part of a service, automatically maintained by a parent service and also platform aware.
- Fully configurable with Scriptable profiles - Each service can host a configuration profile to change the behaviour of your service without changing code.

## Requirements

- [Unity 2021.3 or above](https://unity.com/)
- [RealityCollective.Utilities](https://github.com/realitycollective/com.realitycollective.utilities)

### OpenUPM

The simplest way to getting started using the utilities package in your project is via OpenUPM. Visit [OpenUPM](https://openupm.com/docs/) to learn more about it. Once you have the OpenUPM CLI set up use the following command to add the package to your project:

```text
    openupm add com.realitycollective.service-framework
```

## Use cases

The service framework has been the foundation behind such toolkit's as Microsoft's MRTK and open source projects like the XRTK and newly formed [Reality Toolkit](https://www.realitytoolkit.io/).  These utilise the framework to enable such use cases as:

- A platform independent input system - A single service able to route input data from multiple controllers on various platforms, each controller only activates on the platform it was designed for.
- An Authentication service - Able to integrate with multiple authentication providers as needed through a single interface.
- A networking service - Utilising multiple endpoints for Lobbys, communication routes and data sharing.

The possibilities are almost endless.

---

## Getting Started

### 1. Creating a service

A fully featured "Service Generator" is included with the Service Framework to get you quickly started, by simply giving a service a name and a namespace with which to run from, the generator will quickly create:

- An interface to identify your service to the Service Framework (all services are identified by their parent interface)
- A configuration profile - to customise to the needs of your service (optional, delete if not required)
- The Service Implementation - You service to do with as you wish.

Additionally, the generator can also create additional data providers (sub services) for your service to maintain, these require you to specify the parent services interface when generating to ensure they are appropriately bound in creation.  Data Providers are automatically started with a parent service provided their platforms match the current operating environment.

### 2. Configuring your service

With your service created, it will need to be registered with an active "Service Manager" in a scene, this can either use the provided "Service Manager Instance" component on a GameObject, or uitilised as a private property on a class of your own.

> Note, at this time, only a single Service Framework Manager can be active in the scene at a time.

Simply create an empty GameObject and add the **ServiceManagerInstance** component to it to begin.  From there it is simply a matter of creating a Profile for the Service Manager and then adding your services to it.

### 3. Accessing your running services

Your services are available at any time from anywhere in your code by simply requesting the service from the Service Manager using its interface (Data Providers are also accessible directly, although we recommend working through your service), for example:

```csharp
    var myService = ServiceManager.Instance.GetService<MyServiceInterface>();
```

Alternatively, there are also TryGet versions of the Service endpoints which return a bool to denote the service retrieval was successful and an out parameter to output the service instance, for example:

```csharp
    IService myServiceInstance;
    if(!ServiceManager.Instance.TryGetService<MyServiceInterface>(out myServiceInstance))
    {
        // Do something if your service was not found.
    }
```

---

## Feedback

Please feel free to provide feedback via the [Reality Toolkit dev channel here](https://github.com/realitycollective/com.realitycollective.service-framework/issues), all feedback. suggestions and fixes are welcome.

---

## Related Articles

- [Welcome to the Service Framework](https://service-framework.realitycollective.io/docs/get-started)
- [Introduction](https://service-framework.realitycollective.io/docs/basics/introduction)
- [Creating your first service](https://service-framework.realitycollective.io/docs/basics/getting_started)
- [Roadmap](https://service-framework.realitycollective.io/docs/basics/roadmap)

---

## Raise an Information Request

If there is anything not mentioned in this document or you simply want to know more, raise an [RFI (Request for Information) request here](https://github.com/realitycollective/com.realitycollective.service-framework/issues/new?assignees=&labels=question&template=request_for_information.md).

Or simply [**join us on Discord**](https://discord.gg/YjHAQD2XT8) and come chat about your questions, we would love to hear from you