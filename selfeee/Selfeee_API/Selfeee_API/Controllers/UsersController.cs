using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Selfeee_API;

namespace Selfeee_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DBInteractor _context;

        public UsersController(DBInteractor context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/isAlive
        [HttpGet("isAlive")]
        public async Task<ActionResult<Users>> IsAlive()
        {          
            return Ok();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUsers(long id)
        {
            var users = await _context.Users.FindAsync(id);

            if (users == null)
            {
                return NotFound();
            }

            return users;
        }

        // GET: api/Users/name:passwd
        [HttpGet("{userName}:{password}")]
        public async Task<ActionResult<Users>> GetUsersPasswd(string userName, string password)
        {
            var users = await _context.Users.Where(x => x.UserName == userName).FirstAsync();

            if (users == null || users.Password != password)
            {
                return Unauthorized();
            }

            return users;
        }

        // GET: api/Users/e:{name}
        [HttpGet("e_{userName}")]
        public async Task<bool> CheckIfExists(string userName)
        {
            try
            {
                var users = await _context.Users.Where(x => x.UserName == userName).FirstOrDefaultAsync();
                if (users == null)
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }          
        }

        // GET: api/Users/gi_{id}
        [HttpGet("gi_{id}")]
        public async Task<byte[][]> GetImages(int id)
        {
            try
            {
                var users = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (users != null)
                {
                    string subDir = users.ImagesPath;
                    if (!Directory.Exists(subDir))
                    {
                        return null;
                    }

                    string[] filePaths = Directory.GetFiles(subDir + "\\");
                    List<Bitmap> imagesList = new List<Bitmap>();
                    foreach (string imgPath in filePaths)
                        imagesList.Add(new Bitmap(imgPath));

                    byte[][] imageBytes = ByteBitmapConverter.BitmapToByteArray(imagesList);

                    foreach (var tmp in imagesList)
                        tmp.Dispose();
                    imagesList.Clear();
                    imagesList = null;

                    return imageBytes;
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(long id, Users users)
        {
            if (id != users.Id)
            {
                return BadRequest();
            }

            _context.Entry(users).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPut("ppi{id}")]
        public async Task<IActionResult> PutProfImg(long id, byte[] img)
        {
            var result = _context.Users.SingleOrDefault(b => b.Id == id);
            if (result != null)
            {
                result.ImageProfile = img;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPut("pis{id}")]
        public async Task<IActionResult> PutImages(long id, byte[][] imgs)
        {
            try
            {
                var result = _context.Users.SingleOrDefault(b => b.Id == id);
                if (result != null)
                {
                    string subDir = result.ImagesPath;
                    Random r = new Random();
                    if (!Directory.Exists(subDir))
                    {
                        Directory.CreateDirectory(subDir);                       
                    }

                    List<Bitmap> images = ByteBitmapConverter.ByteToBitmapList(imgs);

                    string[] filePaths = Directory.GetFiles(subDir+"\\");
                    foreach (string filePath in filePaths)
                        System.IO.File.Delete(filePath);

                    foreach (var tmp in images)
                    {
                        string path = subDir + "\\" + DateTime.Now.Ticks.ToString() + ".Jpeg";
                        
                        using (Bitmap bitmap = new Bitmap(tmp.Width, tmp.Height, tmp.PixelFormat))
                        {
                            Graphics g = Graphics.FromImage(bitmap);
                            g.DrawImage(tmp, new Point(0, 0));
                            bitmap.Save(path, ImageFormat.Jpeg);
                            g.Dispose();
                            tmp.Dispose();
                            bitmap.Dispose();                          
                        }
                    }
                    for(int i =0; i < images.Count; i++)
                    {
                        images[i].Dispose();
                        images[i] = null;
                    }
                    images.Clear();
                    images = null;
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            await Task.Yield();
            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Users>> PostUsers(Users users)
        {
            _context.Users.Add(users);
            await _context.SaveChangesAsync();
            var result = _context.Users.SingleOrDefault(b => b.UserName == users.UserName);
            string dir = @"E:\selfeee_db\images";
            string subDir = string.Format(@"E:\selfeee_db\images\{0}", result.Id + "_" + result.UserName);
            result.ImagesPath = subDir;
            await _context.SaveChangesAsync();

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            Directory.CreateDirectory(subDir);
            return CreatedAtAction("GetUsers", new { id = users.Id }, users);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Users>> DeleteUsers(long id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            _context.Users.Remove(users);
            await _context.SaveChangesAsync();

            return users;
        }

        private bool UsersExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
