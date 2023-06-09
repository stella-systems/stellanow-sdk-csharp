/*
 *  This file is auto-generated. DO NOT EDIT.
 *  All rights reserved to Stella Technologies (UK) Limited
 */

using StellaNowSDK.Messages;

namespace StellaNowSDKDemo.Messages;

public class UserLoginMessage : StellaNowMessageWrapper
{
    public UserLoginMessage(string patronId, string user_id, string timestamp)
        : base(
            "user_login",
            new List<EntityType>{ new EntityType("patron", patronId) })
    {
        AddField("user_id", user_id);
        AddField("timestamp", timestamp);
    }
}

/*
Generated from:

{
    "id": "78aa3dfe-7ab3-4a6b-8eae-b16bfefdb8f4",
    "name": "user_login",
    "description": "",
    "isActive": true,
    "createdAt": "2023-06-07T07:21:53.687903Z",
    "updatedAt": "2023-06-07T08:17:35.259619Z",
    "fields": [
        {
            "id": "38ad3ad2-c6b2-43c3-87c8-100ef5fc75a7",
            "name": "user_id",
            "valueType": "String"
        },
        {
            "id": "b6d1f337-9438-44aa-827b-9a074c364a64",
            "name": "timestamp",
            "valueType": "String"
        }
    ],
    "entities": [
        {
            "id": "8a29baf4-a157-4c39-ac68-61c16eb0e6c1",
            "name": "patron"
        }
    ]
}
*/