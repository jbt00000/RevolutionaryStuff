# "REST" Api Clients
Instead of making the frontend dependent on a bunch of backend projects, instead we want to simply import their swagger docs
This unfortunately is a lot *harder* said than done.

# Refitter

Seems to be the best of the worst options...

    dotnet tool install -g Refitter

    refitter --skip-validation -s .refitter 

    https://github.com/christianhelle/refitter

    https://github.com/reactiveui/refit

    https://localhost:7243/openapi/v1.json

But... after you generate, you still need to MANUALLY update the generated C# Interface

Below is my discourse with ChatGPT (which eventually, Bard agreed with)

    Me:
    I'm trying to use refitter to generate a C# client from a swagger document, but it's not working.  I'm getting a bunch of errors like this:can you show me an example of an openAPI json document that would cause refitter to generate the ["Authorization: Bearer")]

    ChATGPT: 
    Refit doesn't automatically generate the ["Authorization: Bearer"] header annotation directly from an OpenAPI (Swagger) JSON document. The process of adding authorization headers, like "Authorization: Bearer", is typically done manually in the Refit interface definitions.
    OpenAPI documents describe the endpoints, parameters, and other aspects of a REST API, but they don't directly translate into header annotations in a Refit-generated interface. When you create the Refit interfaces based on the OpenAPI spec, you need to manually add any required headers, such as the Authorization header, based on your understanding of the API's authentication requirements.
    If you want the generated clients to include specific headers like the Authorization header, you would typically add these headers to your Refit interfaces after generating the basic structure of the interfaces from the OpenAPI spec. This can be part of a custom post-processing step after generating the client code.
