// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace IdentityServer4.SelfHost.Models
{
    public class LoginViewModel : LoginInputModel
    {
        public LoginViewModel()
        {            
        }

        public LoginViewModel(LoginInputModel other)
        {
            Username = other.Username;
            Password = other.Password;
            ReturnUrl = other.ReturnUrl;
        }

        public string ErrorMessage { get; set; }
    }
}
