using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Support.V7.App;
using Android.Widget;
using Newtonsoft.Json;


namespace selfeee_cam
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        TextView textViewLogin;
        EditText txtusername;
        EditText txtPassword;
        Button btncreate;
        Button btnsign;
        Button btnAnonymous;
        CheckBox chRemember;       

        private ISharedPreferences _sharedPreferences;
        private const string LoginLabel = "SavedLogin";
        private const string PasswordLabel = "SavedPassword";
        private const string isRemember = "SavedRemeberState";

        static readonly HttpClient client = new HttpClient();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LoginWindow);

            textViewLogin = FindViewById<TextView>(Resource.Id.txtviewLogin);
            btnsign = FindViewById<Button>(Resource.Id.btnLogin);
            btncreate = FindViewById<Button>(Resource.Id.btnNewUser);
            btnAnonymous = FindViewById<Button>(Resource.Id.btnAnonymous);
            txtusername = FindViewById<EditText>(Resource.Id.editNameLogin);
            txtPassword = FindViewById<EditText>(Resource.Id.editPasswordLogin);
            chRemember = FindViewById<CheckBox>(Resource.Id.checkBoxRemeberMe);

            btnAnonymous.Click += BtnAnonymous_Click;
            btnsign.Click += Btnsign_Click;
            btncreate.Click += Btncreate_Click;

            Typeface typeface = Typeface.CreateFromAsset(Assets, "Satisfy-Regular.ttf");
            textViewLogin.SetTypeface(typeface, TypefaceStyle.Normal);
            btncreate.SetTypeface(typeface, TypefaceStyle.Normal);
            btnsign.SetTypeface(typeface, TypefaceStyle.Normal);
            btnAnonymous.SetTypeface(typeface, TypefaceStyle.Normal);
            chRemember.SetTypeface(typeface, TypefaceStyle.Normal);

            //CreateDB();

            _sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
            if (_sharedPreferences.GetBoolean(isRemember, false))
            {
                chRemember.Checked = true;
                txtusername.Text = _sharedPreferences.GetString(LoginLabel, "");
                txtPassword.Text = _sharedPreferences.GetString(PasswordLabel, "");
            }
        }

        protected override void OnPause()
        {
            ISharedPreferencesEditor editor = this._sharedPreferences.Edit();
            if (chRemember.Checked == true)
            {
                editor.PutString(LoginLabel, txtusername.Text);
                editor.PutString(PasswordLabel, txtPassword.Text);              
            }
            else
            {
                editor.PutString(LoginLabel, null);
                editor.PutString(PasswordLabel, null);
            }
            editor.PutBoolean(isRemember, chRemember.Checked);
            editor.Commit();

            base.OnPause();
        }

        private void BtnAnonymous_Click(object sender, EventArgs e)
        {
            MainActivityContext.Anonymous = true;
            StartActivity(typeof(MainActivity));
        }

        private void Btncreate_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(RegisterActivity));
        }
        private async void Btnsign_Click(object sender, EventArgs e)
        {
            try
            {
                //string dpPath = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "user.db3"); //Call Database  
                //var db = new SQLiteConnection(dpPath);
                //var data = db.Table<LoginTable>(); //Call Table  
                //var data1 = data.Where(x => x.username == txtusername.Text && x.password == txtPassword.Text).FirstOrDefault(); //Linq Query
                var data1 = await GetUserAsync(txtusername.Text, txtPassword.Text);                
                if (data1 != null)
                {
                    LoginTable bsObj = JsonConvert.DeserializeObject<LoginTable>(data1);
                    Toast.MakeText(this, "Login Success", ToastLength.Short).Show();
                    MainActivityContext.userName = bsObj.username;
                    MainActivityContext.id = bsObj.id;
                    MainActivityContext.userData = bsObj;
                    if(bsObj.imageprofile != null)
                        PrivatePhotosBitmap.UserProfileImage = BitmapFactory.DecodeByteArray(bsObj.imageprofile, 0, bsObj.imageprofile.Length);
                    StartActivity(typeof(MainActivity));
                }
                //else
                //{
                //    Toast.MakeText(this, "Username or Password invalid", ToastLength.Short).Show();
                //}
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.ToString(), ToastLength.Short).Show();
            }
        }
        //public string CreateDB()
        //{
        //    var output = "";
        //    output += "Creating Databse if it doesnt exists";
        //    string dpPath = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "user.db3"); //Create New Database  
        //    var db = new SQLiteConnection(dpPath);
        //    output += "\n Database Created....";
        //    return output;
        //}

        private async Task<string> GetUserAsync(string username, string password)
        {
            string getPath = string.Format(@"http://192.168.0.103:120/api/users/{0}:{1}", username, password);
            HttpResponseMessage response = await client.GetAsync(getPath);
            response.EnsureSuccessStatusCode();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Toast.MakeText(this, "Invalid username or password", ToastLength.Short).Show();
                return null;
            }
            else
            {
                Toast.MakeText(this, "Server is unavailable", ToastLength.Short).Show();
                return null;
            }
        }
    }
}