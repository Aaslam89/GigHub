using GigHub.Models;
using GigHub.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace GigHub.Controllers
{
    public class GigsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GigsController()
        {
            _context = new ApplicationDbContext();
        }

        [Authorize]
        public ActionResult Mine()
        {
            var userId = User.Identity.GetUserId();
            var gigs = _context.Gigs
                .Where(x => x.ArtistId == userId && x.DateTime > DateTime.Now)
                .Include(x => x.Genre)
                .ToList();

            return View(gigs);
        }

        [Authorize]
        public ActionResult Attending()
        {
            var userId = User.Identity.GetUserId();
            var gigs = _context.Attendances
                .Where(x => x.AttendeeId == userId)
                .Select(x=>x.Gig)
                .Include(x=>x.Artist)
                .Include(x=>x.Genre)
                .ToList();

            var viewModel = new GigsViewModel()
            {
                UpComingGigs = gigs,
                ShowActions = User.Identity.IsAuthenticated,
                Heading = "Gigs I'm Attending"
            };
            return View("Gigs", viewModel);
        }
        // GET: Gigs
        [Authorize]
        public ActionResult Create()
        {
            var viewModel = new GigFormViewModel()
            {
                Genres = _context.Genres.ToList()
            };
            return View(viewModel);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            var userId = User.Identity.GetUserId();
            var gig = _context.Gigs.Single(x => x.Id == id && x.ArtistId == userId);

            var viewModel = new GigFormViewModel()
            {
                Genres = _context.Genres.ToList()
            };
            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GigFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = _context.Genres.ToList();
                return View("Create", model);
            }
            else
            {
                var gig = new Gig()
                {
                    ArtistId = User.Identity.GetUserId(),
                    DateTime = model.GetDateTime(),
                    GenreId = model.Genre,
                    Venue = model.Venue
                };

                _context.Gigs.Add(gig);
                _context.SaveChanges();
                return RedirectToAction("Mine", "Gigs");
            }           
        }
    }
}