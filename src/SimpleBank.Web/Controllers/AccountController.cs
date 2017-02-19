using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimpleBank.Service.Entities;
using SimpleBank.Web.Data;
using Microsoft.EntityFrameworkCore;
using SimpleBank.Web.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SimpleBank.Service.Data;
using SimpleBank.Service.IServices;
using SimpleBank.Service.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SimpleBank.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITransferService _transferService;

        public AccountController(
            IUserService userService,
            ITransferService transferService
            )
        {
            _transferService = transferService;
            _userService = userService;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var lstAccount = await _userService.GetListAllUser();

            return View(lstAccount);
        }

        // Create new Account
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(AccountViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    viewModel.User.CreatedDate = DateTime.Now;

                    await _userService.CreateNewUser(viewModel.User);

                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to create an Account.");
            }

            return View(viewModel);
        }

        // Deposite amount
        [HttpGet]
        public async Task<IActionResult> Deposite(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetAccountById(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new AccountViewModel
            {
                User = user
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Deposite(AccountViewModel viewModel)
        {
            BankUserUpdateModel result = await _userService.Deposit(viewModel.User.ID, viewModel.DepositeAmount, viewModel.User.Timestamp);

            if (result.ErrorList.Any())
            {
                foreach(var error in result.ErrorList)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
                viewModel.User.Timestamp = null;
                viewModel.User.Timestamp = result.EntityTimestamp;
                // remove from ModelState to force User.Timestamp to take new value
                ModelState.Remove("User.Timestamp");
            }
            else
            {
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        // Withdraw amount
        [HttpGet]
        public async Task<IActionResult> Withdraw(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetAccountById(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new AccountViewModel
            {
                User = user
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Withdraw(AccountViewModel viewModel)
        {
            BankUserUpdateModel result = await _userService.Withdraw(viewModel.User.ID, viewModel.WithdrawAmount, viewModel.User.Timestamp);

            if (result.ErrorList.Any())
            {
                foreach (var error in result.ErrorList)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
                viewModel.User.Timestamp = null;
                viewModel.User.Timestamp = result.EntityTimestamp;
                // remove from ModelState to force User.Timestamp to take new value
                ModelState.Remove("User.Timestamp");
            }
            else
            {
                return RedirectToAction("Index");
            }
            
            return View(viewModel);
        }

        // Transfer amount
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Transfer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toUser = await _userService.GetAccountById(id.Value);
            if (toUser == null)
            {
                return NotFound();
            }

            var loginUserId = int.Parse(this.User.FindFirst("UserId").Value);
            var fromUser = await _userService.GetAccountById(loginUserId);
            if (fromUser == null)
            {
                return NotFound();
            }

            var viewModel = new TransferViewModel
            {
                FromUser = fromUser,
                ToUser = toUser,
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Transfer(TransferViewModel viewModel)
        {
            BankUserUpdateModel result = await _userService.Transfer(
                viewModel.FromUser.ID, 
                viewModel.ToUser.ID, 
                viewModel.TranferAmount, 
                viewModel.FromUser.Timestamp, 
                viewModel.ToUser.Timestamp);

            if (result.ErrorList.Any())
            {
                foreach (var error in result.ErrorList)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }
            else
            {
                return RedirectToAction("Index");
            }


            return View(viewModel);
        }

        // Transfer history
        [HttpGet]
        public async Task<IActionResult> TransferHistory(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var lstTransferHistory = await _transferService.GetListTransferHistoryByAccountId(id.Value);

            return View(lstTransferHistory);
        }
    }
}
