using BookStoreAPI.Models;
using BookStoreAPI.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreAPI.Areas.Identity
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area("Identity")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<PersonInfo> _PersonInfo;
        public ProfileController(UserManager<ApplicationUser> userManager, IRepository<PersonInfo> personInfo)
        {
            _userManager = userManager;
            _PersonInfo = personInfo;
        }



        [HttpGet ("Get")]
        public async Task<IActionResult> Get()
        {
            // to get the data from the coockie "User"
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();
            // to send all the user information in a single code using Mapster
          return Ok(user.Adapt<ApplicationUserResponse>());
        }

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(ApplicationUserRequest applicationUserRequest, IFormFile Img)
        {

            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();
            if (applicationUserRequest.ImgFile != null)
            {
                var uploadFolder = Path.Combine("wwwroot/images/User_images");

                if (!string.IsNullOrEmpty(user.Img))
                {
                    var oldPath = Path.Combine(uploadFolder, user.Img);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
                var newFileName = Guid.NewGuid() + Path.GetExtension(applicationUserRequest.ImgFile.FileName);
                var newFilePath = Path.Combine(uploadFolder, newFileName);
                using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                {
                    await applicationUserRequest.ImgFile.CopyToAsync(fileStream);
                }
                user.Img = newFileName;
            }
            user.Name = applicationUserRequest.Name;
            user.UserName = applicationUserRequest.UserName;
            user.Email = applicationUserRequest.Email;
            user.PhoneNumber = applicationUserRequest.PhoneNumber;
            user.Address = applicationUserRequest.Address;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return NoContent();
            }
            return NoContent();
        }

        [HttpPost ("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ApplicationUserRequest applicationUserRequest)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, applicationUserRequest.OldPassword, applicationUserRequest.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return RedirectToAction(nameof(Index));
        }


    }
}
