﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjektIO.Models;
using Microsoft.AspNet.Identity;

namespace ProjektIO.Controllers
{
    public class PostController : Controller
    {
        // GET: Post
        public ActionResult Index()
        {
            return View();
        }

        
        public ActionResult AddPost()
        {
            using (var db = new DatabaseContext())
            {
                ViewModels viewModel = new ViewModels();
                int userId = (User as ProjektIO.Auth.Principal).GetUserData().Id;
                Czlonkowie member = db.Czlonkowie.FirstOrDefault(p => p.IdUzytkownika == userId);
                viewModel.AddPost.Member = member;
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult AddPost(ViewModels viewModel)
        {
            using (var db = new DatabaseContext())
            {
                Post post = new Post();
                post.IdCzlonka = viewModel.AddPost.Member.Id;
                post.Przypiety = false;
                post.DataUtworzenia = DateTime.Now;
                post.Zawartosc = viewModel.AddPost.PostContent;
                post.IdKola = viewModel.AddPost.Member.IdKola;
                post.KoloNaukowe = db.KoloNaukowe.Find(viewModel.AddPost.Member.IdKola);
                post.Czlonkowie = db.Czlonkowie.Find(viewModel.AddPost.Member.Id);
                db.Post.Add(post);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        
        //id to id postu
        public ActionResult AddComment(int id)
        {
           ViewModels viewModel = new ViewModels();
            viewModel.AddComment.PostId = id;
           return View(viewModel);
        }

        [HttpPost]
        public ActionResult AddComment (ViewModels viewModel)
        {
            using (var db = new DatabaseContext())
            {
                int id = (User as ProjektIO.Auth.Principal).GetUserData().Id;
                Czlonkowie member = db.Czlonkowie.FirstOrDefault(p => p.IdUzytkownika == id);
                Post post = db.Post.Find(viewModel.AddComment.PostId);
                if (member == null || post == null)
                {
                    return View("Error");
                }
                viewModel.AddComment.Comment.IdPostu = post.Id;
                viewModel.AddComment.Comment.Post = post;
                viewModel.AddComment.Comment.IdCzlonka = member.Id;
                viewModel.AddComment.Comment.Czlonkowie = member;
                db.Komentarz.Add(viewModel.AddComment.Comment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        //przekazuje id posta
        public ActionResult ShowPost(int id, int page = 1)
        {
            int PageSize = 5;

            using (var db = new DatabaseContext())
            {
                PostViewModel viewModel = new PostViewModel();
                viewModel.Post = db.Post.Find(id);
                viewModel.Comments = db.Komentarz.Where(p => p.IdPostu == id).ToList();
                Czlonkowie author = db.Czlonkowie.Include("Uzytkownik").FirstOrDefault(p => p.Id == viewModel.Post.IdCzlonka);
                viewModel.AuthorName = author.Uzytkownik.Imie + " " + author.Uzytkownik.Nazwisko;
                if (viewModel.Post == null || author == null)
                {
                    return View("Error");
                }
                int totalItems = db.Komentarz.Where(p => p.IdPostu == id).ToList().Count();
                viewModel.Pages = (int)Math.Ceiling((decimal)totalItems / PageSize);
                viewModel.CurrentPage = page;
                viewModel = SetCommentsAuthors(viewModel);
                return View(viewModel);
            }
        }

        private PostViewModel SetCommentsAuthors (PostViewModel postModel)
        {
            using (var db = new DatabaseContext())
            {
                foreach (Komentarz comment in postModel.Comments)
                {
                    string temp;
                    Czlonkowie author = db.Czlonkowie.Include("Uzytkownik").FirstOrDefault(p => p.Id == comment.IdCzlonka);
                    temp = author.Uzytkownik.Imie + " " + author.Uzytkownik.Nazwisko;
                    postModel.CommentsAuthors.Add(temp);
                }
                return postModel;
            }
        }
    }
}