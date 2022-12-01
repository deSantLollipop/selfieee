using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;

namespace selfeee_cam
{
    [Activity(Label = "RegisterActivity")]
    public class RegisterActivity : Activity
    {
        TextView txtRegister;
        EditText txtusername;
        EditText txtPassword;
        Button btncreate;

        static readonly HttpClient client = new HttpClient();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Newuser);
            // Create your application here  
            txtRegister = FindViewById<TextView>(Resource.Id.txtviewRegister);
            btncreate = FindViewById<Button>(Resource.Id.btnRegister);
            txtusername = FindViewById<EditText>(Resource.Id.editName);
            txtPassword = FindViewById<EditText>(Resource.Id.editPassword);
            btncreate.Click += Btncreate_Click;


            Typeface typeface = Typeface.CreateFromAsset(Assets, "Satisfy-Regular.ttf");
            txtRegister.SetTypeface(typeface, TypefaceStyle.Normal);
            btncreate.SetTypeface(typeface, TypefaceStyle.Normal);
        }
        private async void Btncreate_Click(object sender, EventArgs e)
        {
            try
            {
                //string dpPath = System.IO.Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "user.db3");
                //var db = new SQLiteConnection(dpPath);

                //var data = db.Table<LoginTable>(); //Call Table  
                //var data1 = data.Where(x => x.username == txtusername.Text).FirstOrDefault(); //Linq Query  
                bool result = await CheckUserAsync(txtusername.Text);
                if (result == true)
                {
                    Toast.MakeText(this, "User already exists", ToastLength.Short).Show();
                }
                else
                {
                    LoginTable newUser = new LoginTable();
                    newUser.username = txtusername.Text;
                    newUser.password = txtPassword.Text;
                    newUser.imageprofile = null;

                    string tmp = JsonConvert.SerializeObject(newUser);
                    bool created = await CreateUserAsync(tmp);
                    //db.CreateTable<LoginTable>();
                    //LoginTable tbl = new LoginTable();
                    //tbl.username = txtusername.Text;
                    //tbl.password = txtPassword.Text;
                    //tbl.imageprofile = null;
                    //db.Insert(tbl);
                    //Toast.MakeText(this, "Record Added Successfully...", ToastLength.Short).Show();
                    ///  
                    if (created == true)
                        StartActivity(typeof(LoginActivity));
                    else
                        Toast.MakeText(this, "Failed", ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.ToString(), ToastLength.Short).Show();
            }
        }

        private async Task<bool> CheckUserAsync(string username)
        {
            string getPath = string.Format(@"http://192.168.0.103:120/api/users/e_{0}", username);
            HttpResponseMessage response = await client.GetAsync(getPath);
            response.EnsureSuccessStatusCode();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                if (responseBody == "true")
                    return true;
                else
                    return false;
            }
            else
                return false;           
        }

        private async Task<bool> CreateUserAsync(string data)
        {
            string putPath = string.Format(@"http://192.168.0.103:120/api/users/");
            var content = new StringContent(data, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(putPath, content);
                response.EnsureSuccessStatusCode();
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    Toast.MakeText(this, "Succeed", ToastLength.Short).Show();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                Toast.MakeText(this, e.ToString(), ToastLength.Short).Show();
                return false;
            }

        }
    }
}