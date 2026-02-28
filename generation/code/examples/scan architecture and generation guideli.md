scan architecture and generation guidelines
note that right now we have separatly blueprint of bondedContext and DomainModel spec, that containes spec of BoundedContexts - this separation remains

new aspect that I want to add for generation tool - authorization within generated domain model.

I want to have multiple authorization resolutions that are resolved during generation of boundedContexts:
 - perDomain
 - perContext
 - none
these are defined in a blueprint

also, in domain model we need to add authorization model, which will provide all necessary details of exact generated authorization mechanisms. for now, auth model will be predefined (see Inventorization.Auth.BL)

some details:
for a temlate of Startup/Program classes extract separate classes (static and reusable) for registration of Auth services/middlewares etc.
whenever perDomain auth is selected:
 - predefined dedicated auth service should be used (Inventorization.Auth.API for now)
 - each bounded context utializes it for auth (permision validation and other common functionalities/guards)
whenever perContext auth is selected:
 - generated bounded contexts should have dedicated endpoints (minimal api) for authentication within that bounded context exclusivly
 - generated bounded contexts should have dedicated entities in a data model for authentication within that bounded context
 - however, the authentication classes (minimal API delegates, their DTOs, Auth Logic services, entities etc.) are not generated per bounded context but rather reused from Inventorization.Auth.* libs
whenever none auth is selected:
 - there is still an Authorization defined in the api (to have templates of the classes to be agnostic to auth method), however it's fully anonymous

plan adjustment for generation process and extention of DomainModel and blueprint. also we might need to extend not only the generation tool, but the dotnet projects foundations, analyse if we need to have our own Authorization Attribute and Auth Middleware classes (it might be usefull to extend Ayth mechanisms in future). Review current Auth projects- how can they be improved?

