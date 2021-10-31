using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ScreenDraw.Pages
{
    public class SketchRoomModel : PageModel
    {
        private readonly ILogger<SketchRoomModel> _logger;

        public SketchRoomModel(ILogger<SketchRoomModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(string RoomName)
        {

        }


    }
}
