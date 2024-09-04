# Sample project for on-behalf-of flow flow

This repository is used as a sample to create the OAuth2.0 On-Behalf-Of flow using two different .NET Core application and Entra ID.  
The flow and detals are descibed in the [MS Learn pages regarding this topic](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-on-behalf-of-flow).

## Set up

This solution contains multiple projects:

* BackendService  
  This service has authorization in place for all endpoints.
* IntegratingService
  This service also has authorization in place, but also needs to interact with the `BackendService` using the OAuth2.0 OBO-flow to retrieve data for the user.

## Sample usage

```http
GET http://localhost:8080/get-my-data
```
