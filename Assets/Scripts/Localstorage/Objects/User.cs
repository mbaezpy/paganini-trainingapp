using System.Collections.Generic;
using SQLite4Unity3d;

public class User : BaseModel<User>
{
    [PrimaryKey]
    public int Id { set; get; }
    public string Mnemonic_token { set; get; }
    public string Username { set; get; }
    public string Apitoken { set; get; }
    public string AppName { set; get; }
    public bool Righthanded { set; get; }
    public bool Canread { set; get; }
    public bool Activatetts { set; get; }
    public bool Vibration { set; get; }
    public string Contact { set; get; }
    public int WorkshopId { set; get; }
    // This is a local variable that is not sent to the API
    public byte[] ProfilePic { set; get; }
    [Ignore]
    public Workshop AtWorkshop { set; get; }

    public override string ToString()
    {
        return string.Format("[User: user_id={0}, user_mnemonic_token={1}, user_username={2}, app_name={8}, user_apitoken={3}, user_righthanded={4}, user_canread={5}, user_activatetts={6}, user_vibration={7}]", Id, Mnemonic_token, Username, Apitoken, Righthanded, Canread, Activatetts, Vibration, AppName);
    }

    public User() { }
    public User(IUserAPI profil)
    {
        this.Id = profil.user_id;
        this.Mnemonic_token = profil.user_mnemonic_token;
        this.Username = profil.user_username;
        this.Apitoken = profil.user_apitoken;
        this.Righthanded = profil.user_righthanded;
        this.Canread = profil.user_canread;
        this.Activatetts = profil.user_activatetts;
        this.Vibration = profil.user_vibration;
        this.Contact = profil.user_contact;

        if (profil.user_workshop != null)
        {
            AtWorkshop = new Workshop(profil.user_workshop);
            WorkshopId = AtWorkshop.Id;        
        }     

        this.FromAPI = true;   
    }

    public UserAPI ToAPI()
    {
        UserAPI user = new UserAPI
        {
            user_id = this.Id,
            user_mnemonic_token = this.Mnemonic_token,
            user_username = this.Username,
            user_apitoken = this.Apitoken,
            user_righthanded = this.Righthanded,
            user_canread = this.Canread,
            user_activatetts = this.Activatetts,
            user_vibration = this.Vibration,
            user_contact = this.Contact
        };

        user.IsNew = !FromAPI;

        return user;
    }

    public static List<User> ToModelList(UserAPIList collection)
    {
        List<User> list = new List<User> { };

        foreach (var userAPI in collection.users)
        {
            list.Add(new User(userAPI));
        }
        return list;
    }
}