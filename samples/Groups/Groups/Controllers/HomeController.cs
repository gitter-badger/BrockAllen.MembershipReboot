﻿using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace RolesAdmin.Controllers
{
    public class GroupIndexViewModel
    {
        public IEnumerable<GroupViewModel> Groups { get; set; }
        public IEnumerable<SelectListItem> GroupsAsList
        {
            get
            {
                return Groups.Select(x=>new SelectListItem { Text=x.Name, Value=x.ID.ToString() });
            }
        }
    }

    public class GroupViewModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public IEnumerable<GroupViewModel> Children { get; set; }
        public IEnumerable<string> Descendants { get; set; }
    }

    public class HomeController : Controller
    {
        GroupService groupSvc;
        IGroupQuery query;
        public HomeController(GroupService groupSvc, IGroupQuery query)
        {
            this.groupSvc = groupSvc;
            this.query = query;
        }

        public ActionResult Index(string filter = null)
        {
            var list = new List<GroupViewModel>();
            foreach (var result in query.Query(groupSvc.DefaultTenant, filter))
            {
                var item = groupSvc.Get(result.ID);
                var kids = new List<GroupViewModel>();
                foreach (var child in item.Children)
                {
                    var childGrp = groupSvc.Get(child.ChildGroupID);
                    kids.Add(new GroupViewModel { ID = child.ChildGroupID, Name = childGrp.Name });
                }
                var descendants = groupSvc.GetDescendants(item).Select(x => x.Name).ToArray();
                var gvm = new GroupViewModel
                {
                    ID = item.ID,
                    Name = item.Name,
                    Children = kids,
                    Descendants = descendants
                };
                list.Add(gvm);
            }
            var vm = new GroupIndexViewModel { 
                Groups = list
            };
            return View("Index", vm);
        }

        [HttpPost]
        public ActionResult Add(string name)
        {
            try
            {
                groupSvc.Create(name);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Index();
        }

        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            try
            {
                groupSvc.Delete(id);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Index();
        }

        [HttpPost]
        public ActionResult ChangeName(Guid id, string name)
        {
            try
            {
                groupSvc.ChangeName(id, name);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Index();
        }

        [HttpPost]
        public ActionResult AddChild(Guid id, Guid child)
        {
            try
            {
                groupSvc.AddChildGroup(id, child);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Index();
        }
        [HttpPost]
        public ActionResult RemoveChild(Guid id, Guid child)
        {
            try
            {
                groupSvc.RemoveChildGroup(id, child);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Index();
        }
    }
}
