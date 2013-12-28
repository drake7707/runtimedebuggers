using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunTimeDebuggers.Helpers
{
    class BookmarkManager
    {

        private static BookmarkManager instance;

        public static BookmarkManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new BookmarkManager();

                return instance;
            }
        }

        private BookmarkManager()
        {
            Bookmarks = new HashSet<object>();
        }

        private  HashSet<object> Bookmarks { get; set; }

        public IEnumerable<object> GetBookMarks()
        {
            return Bookmarks;
        }

        public void AddBookmark(object obj)
        {
            if (!Bookmarks.Contains(obj))
            {
                Bookmarks.Add(obj);

                BookmarkHandler temp = BookmarkAdded;
                if (temp != null)
                    temp(obj);
            }
        }

        public void RemoveBookmark(object obj)
        {
            if (Bookmarks.Contains(obj))
            {
                Bookmarks.Remove(obj);

                BookmarkHandler temp = BookmarkRemoved;
                if (temp != null)
                    temp(obj);
            }
        }
        
        public event BookmarkHandler BookmarkAdded;
        public event BookmarkHandler BookmarkRemoved;

        public delegate void BookmarkHandler(object bookmark);
        
    }
}
