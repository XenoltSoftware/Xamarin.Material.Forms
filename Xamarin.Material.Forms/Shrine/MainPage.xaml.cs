using System;
using System.Net.Mail;
using Xamarin.Forms;

namespace Shrine
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            if (Email.Text == null)
            {
                Email.Error = "email cannot be empty";
            }
            else
            {
                IsEmailValid(Email.Text);
            }
            if (password.Text == null)
            {
                password.Error = "password cannot be empty";
            }
            else
            {
                if (password.Text.Length > 7)
                {
                    password.Error = "";
                }
                else
                {
                    password.Error = "password must be of minimum 8 characters";
                }
            }
        }

        private bool IsEmailValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                Email.Error = "";
                return true;
            }
            catch (FormatException)
            {
                Email.Error = "email is not Valid";
                return false;
            }
        }
    }
}
