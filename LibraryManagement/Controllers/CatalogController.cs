﻿using LibraryData;
using LibraryManagement.Models.Catalog;
using LibraryManagement.Models.Checkout;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LibraryManagement.Controllers
{
    public class CatalogController : Controller
    {
        private ILibraryAsset _asset;
        private ICheckout _checkouts;

        public CatalogController(ILibraryAsset asset, ICheckout checkout)
        {
            _asset = asset;
            _checkouts = checkout;
        }

        public IActionResult Index()
        {
            var assetModels = _asset.GetAll();

            var listingResult = assetModels
                .Select(result => new AssetIndexListingModel
                {
                    Id = result.Id,
                    ImageUrl = result.ImageUrl,
                    AuthorOrDirector = _asset.GetAuthorOrDirector(result.Id),
                    DeweyCallNumber = _asset.GetDeweyIndex(result.Id),
                    Title = result.Title,
                    Type = _asset.GetType(result.Id)
                });

            var model = new AssetIndexModel()
            {
                Assets = listingResult
            };

            return View(model);
        }
        
        public IActionResult Detail(int id)
        {
            var asset = _asset.GetById(id);

            var curentHolds = _checkouts.GetCurrentHolds(id)
                .Select(a => new AssetHoldModel
                {
                    HoldPlaced = _checkouts.GetCurrentHoldPlaced(a.Id).ToString("d"),
                    PatronName = _checkouts.GetCurrentHoldPatronName(a.Id)
                    
                });

            var model = new AssetDetailModel
            {
                AssetId = id,
                Title = asset.Title,
                Type = _asset.GetType(id),
                Year = asset.Year,
                Cost = asset.Cost,
                Status = asset.Status.Name,
                ImageUrl = asset.ImageUrl,
                AuthorOrDirector = _asset.GetAuthorOrDirector(id),
                CurrentLocation = _asset.GetCurrentLocation(id).Name,
                DeweyCallNumber = _asset.GetDeweyIndex(id),
                CheckoutHistory = _checkouts.GetCheckoutHistory(id),
                ISBN = _asset.GetIsbn(id),
                LatestCheckout = _checkouts.GetLatestCheckout(id),
                PatronName = _checkouts.GetCurrentCheckoutPatron(id),
                CurrentHolds = curentHolds

            };

            return View(model);
        }

        public IActionResult MarkLost(int assetId)
        {
            _checkouts.MarkLost(assetId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        public IActionResult MarkFound(int assetId)
        {
            _checkouts.MarkFound(assetId);
            return RedirectToAction("Detail", new { id = assetId });
        }


        public IActionResult Checkout(int id)
        {
            var asset = _asset.GetById(id);

            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = " ",
                IsCheckedOut = _checkouts.IsCheckedout(id)
            };

            return View(model);
        }

        public IActionResult CheckIn(int id)
        {
            _checkouts.CheckinItem(id);
            return RedirectToAction("Detail", new { id = id });
        }

        public IActionResult Hold (int id)
        {
            var asset = _asset.GetById(id);

            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = " ",
                IsCheckedOut = _checkouts.IsCheckedout(id),
                HoldCount = _checkouts.GetCurrentHolds(id).Count()
                
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult PlaceCheckout(int assetId, int libraryCardId)
        {
            _checkouts.CheckoutItem(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        [HttpPost]
        public IActionResult PlaceHold(int assetId, int libraryCardId)
        {
            _checkouts.PlaceHold(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }
    }

}
