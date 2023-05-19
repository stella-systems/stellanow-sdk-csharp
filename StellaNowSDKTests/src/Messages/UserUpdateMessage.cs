using StellaNowSDK.Messages;

namespace StellaNowSDKTests.Messages;

public class UserUpdateMessage : StellaNowMessageWrapper
{
    public UserUpdateMessage(string punterId, string firstName, string lastName, string dob, string email) 
        : base(
            "user_update", 
            new List<EntityType>{new EntityType("punter", punterId)})
    {
        AddField("firstName", firstName);
        AddField("lastName", lastName);
        AddField("dob", dob);
        AddField("email", email);
    }

}