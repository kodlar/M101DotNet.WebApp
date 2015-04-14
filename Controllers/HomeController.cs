using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using MongoDB.Driver;
using M101DotNet.WebApp.Models;
using M101DotNet.WebApp.Models.Home;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace M101DotNet.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            
            var blogContext = new BlogContext();
            // XXX WORK HERE
            // find the most recent 10 posts and order them
            // from newest to oldest

            //db.posts.ensureIndex({CreatedAtUtc:1})
            //db.posts.find().limit(10).sort({CreatedAtUtc:1}).explain()

            var filter = new BsonDocument();
            var recentPosts = await blogContext.Posts.Find(filter).Limit(10).Sort("{Date:1}").ToListAsync();

            /*
             * Alternatif yazım
             var recentPosts = await blogContext.Posts.Find(x => true)
                .SortByDescending(x => x.CreatedAtUtc)
                .Limit(10)
                .ToListAsync();
             */


            var model = new IndexModel
            {
                RecentPosts = recentPosts
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult NewPost()
        {
            return View(new NewPostModel());
        }

        [HttpPost]
        public async Task<ActionResult> NewPost(NewPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
    
            var blogContext = new BlogContext();
            // XXX WORK HERE
            // Insert the post into the posts collection

            /*Alternatif yazım
             *            var blogContext = new BlogContext();
            var post = new Post
            {
                Author = User.Identity.Name,
                Title = model.Title,
                Content = model.Content,
                Tags = model.Tags.Split(' ', ',', ';'),
                CreatedAtUtc = DateTime.UtcNow,
                Comments = new List<Comment>()
            };

            await blogContext.Posts.InsertOneAsync(post);

             
             */
            

            var post = new Post {CreatedAtUtc = DateTime.UtcNow, Title = model.Title, Content = model.Content, Author = User.Identity.Name};
            string[] words = model.Tags.Split(',');
            post.Tags = new List<string>();
            foreach (string word in words)
            {
                post.Tags.Add(word);
            }

            await blogContext.Posts.InsertOneAsync(post);
            return RedirectToAction("Post", new { id = post.Id });
        }

        [HttpGet]
        public async Task<ActionResult> Post(string id)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find the post with the given identifier
            
            //Alternatif yazım
            //var post = await blogContext.Posts.Find(x => x.Id == id).SingleOrDefaultAsync();
            
            var post = await blogContext.Posts.Find(x => x.Id == ObjectId.Parse(id)).SingleAsync();
            if (post == null)
            {
                return RedirectToAction("Index");
            }

            var model = new PostModel
            {
                Post = post
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Posts(string tag = null)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find all the posts with the given tag if it exists.
            // Otherwise, return all the posts.
            // Each of these results should be in descending order.

            /*Alternatif Yazım
             
              Expression<Func<Post, bool>> filter = x => true;

            if (tag != null)
            {
                filter = x => x.Tags.Contains(tag);
            }

            var posts = await blogContext.Posts.Find(filter)
                .SortByDescending(x => x.CreatedAtUtc)
                .Limit(10)
                .ToListAsync();
             * 
             */


         
         //db.posts.ensureIndex({Tags:1})
         //db.posts.ensureIndex({Tags:1, CreatedAtUtc:1})

            var filter = new BsonDocument("Tags", tag);
            var posts = await blogContext.Posts.Find(filter).Limit(10).ToListAsync();
            return View(posts);
        }

        [HttpPost]
        public async Task<ActionResult> NewComment(NewCommentModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Post", new { id = model.PostId });
            }

            var blogContext = new BlogContext();
            //var comment = new NewCommentModel()
            //{
            //    Content = model.Content,
            //    PostId = model.PostId
            //};

             var tempModel = new Post();
             var tepCommentList = new List<Comment>();
                var comment = new Comment
                {
                    CreatedAtUtc = DateTime.UtcNow,
                    Author = User.Identity.Name,
                    Content = model.Content
                };
                tepCommentList.Add(comment);
            
            tempModel = await blogContext.Posts.Find(x => x.Id == ObjectId.Parse(model.PostId)).SingleAsync();
            if (tempModel.Comments != null)
            {
                tempModel.Comments.Add(comment);    
            }
            else
            {
                tempModel.Comments = tepCommentList;
            }
            
            // XXX WORK HERE
            // add a comment to the post identified by model.PostId.
            // you can get the author from "this.User.Identity.Name"

            /* Alternatif Yazım
             * 
              var comment = new Comment
            {
                Author = User.Identity.Name,
                Content = model.Content,
                CreatedAtUtc = DateTime.UtcNow
            };

            var blogContext = new BlogContext();

            await blogContext.Posts.UpdateOneAsync(
                x => x.Id == model.PostId,
                Builders<Post>.Update.Push(x => x.Comments, comment));

             
             */


            var result = await blogContext.Posts.ReplaceOneAsync(x => x.Id == ObjectId.Parse(model.PostId), tempModel);
            return RedirectToAction("Post", new { id = model.PostId });
        }
    }
}