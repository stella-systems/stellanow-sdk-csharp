/*
 *  This file is auto-generated by StellaNowCLI. DO NOT EDIT.
 *
 *  Event ID: e4267626-ac2c-459f-a86d-5ed81b8e4054
 *  Generated: 2024-11-05T15:10:25Z
 */

using StellaNowSDK.Messages;

using StellaNowSDKDemo.Messages.Models;


namespace StellaNowSDKDemo.Messages;

public record UserDetailsMessage(
    [property: Newtonsoft.Json.JsonIgnore] string patronId,
    [property: Newtonsoft.Json.JsonProperty("user_id")] string UserId,
    [property: Newtonsoft.Json.JsonProperty("phone_number")] PhoneNumberModel PhoneNumber
    ) : StellaNowMessageBase("user_details", new List<EntityType>{ new EntityType("patron", patronId) });

/*
Generated from:

{
    "createdAt": "2024-11-04 17:23:48",
    "updatedAt": "2024-11-04 17:23:52",
    "id": "e4267626-ac2c-459f-a86d-5ed81b8e4054",
    "name": "user_details",
    "projectId": "9c9da21c-6c12-46e7-8e36-0b706ad99898",
    "isActive": true,
    "description": "",
    "fields": [
        {
            "id": "6be39bd4-cccc-46b4-bf89-f9186e2215da",
            "name": "user_id",
            "fieldType": {
                "value": "String"
            },
            "required": true,
            "subfields": []
        },
        {
            "id": "17ca7829-96cb-4429-b0b2-5d8b72a5de0b",
            "name": "phone_number",
            "fieldType": {
                "value": "Model",
                "modelRef": "46f4777b-d622-4e90-8c0c-ddc3e29da906"
            },
            "required": false,
            "subfields": [
                {
                    "id": "1d04120a-6286-41a4-bac5-03e14bd48982",
                    "name": "number",
                    "fieldType": {
                        "value": "Integer"
                    },
                    "required": true,
                    "path": [
                        "phone_number",
                        "number"
                    ],
                    "modelFieldId": "81a4ef9c-5538-488d-bcbc-2d714aadabe9"
                },
                {
                    "id": "319e8784-a09f-4ef3-84b4-7a7cb529e1e2",
                    "name": "country_code",
                    "fieldType": {
                        "value": "Integer"
                    },
                    "required": true,
                    "path": [
                        "phone_number",
                        "country_code"
                    ],
                    "modelFieldId": "b62568d7-0c9f-4af2-8775-f0a975caf534"
                }
            ]
        }
    ],
    "entities": [
        {
            "id": "af0c4d9e-ffec-4484-bd28-acbbce73d927",
            "name": "patron"
        }
    ]
}
*/