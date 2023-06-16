using AspForum.Data;
using AspForum.Data.Entities;
using AspForum.Models.Rate;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspForum.Controllers
{
	[Route("api/[controller]")]
	public class RateController : ControllerBase
	{
		private readonly ApplicationContext _context;
		private readonly UserManager<User> _userManager;

		public RateController(ApplicationContext context, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpPost]
		public async Task<object> Post([FromBody] RateRequestData data)
		{

			if (data == null || data.Rating == 0 || data.Type == null)
			{
				HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
				return $"Not all data provided: Data={data?.Rating}, ItemId={data?.ItemId}, Type={data?.Type}";
			}

			if (data.Type != "Post")
			{
				HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
				return "Invalid type";
			}

			if (!_context.Posts.Any(p => p.Id == data.ItemId) && !_context.Topics.Any(p => p.Id == data.ItemId)) // TODO need better implementation
			{
				HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
				return "Invalid ItemId";
			}

			if (User.Identity is not null && User.Identity.IsAuthenticated &&
				await _userManager.FindByNameAsync(User.Identity.Name) is User user)
			{
				try
				{
					int rating = data.Rating > 0 ? 1 : -1;

					Rate? rate = _context.Rates.FirstOrDefault(r => r.UserId == user.Id && r.ItemId == data.ItemId);
					if (rate is not null)
					{
						if (rate.Rating == rating)
						{
							HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
							return $"Data already exists: ItemId={rate.ItemId}, UserId={rate.UserId}, Rating={rate.Rating}";
						}
						else
						{
							rate.Rating = rating;
							_context.SaveChanges();
							return $"Data changed: ItemId={rate.ItemId}, UserId={rate.UserId}, Rating={rate.Rating}";
						}
					}
					else
					{
						_context.Rates.Add(new()
						{
							ItemId = data.ItemId,
							UserId = user.Id,
							Rating = rating,
						});
						_context.SaveChanges();
						HttpContext.Response.StatusCode = StatusCodes.Status201Created;
						return $"Data added: ItemId={data.ItemId}, UserId={user.Id}, Rating={rating}";
					}
				}
				catch (Exception)
				{
					HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
					return $"Data not processed: Data={data?.Rating}, ItemId={data?.ItemId}, Type={data?.Type}";
				}
			}
			else
			{
				HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
				return "Not authorized";
			}
		}

		[HttpDelete]
		public async Task<object> Delete([FromBody] RateRequestData data)
		{

			if (data == null || data.Type == null)
			{
				HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
				return $"Not all data provided: ItemId={data?.ItemId}, Type={data?.Type}";
			}

			if (data.Type != "Post")
			{
				HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
				return "Invalid type";
			}

			if (!_context.Posts.Any(p => p.Id == data.ItemId) && !_context.Topics.Any(p => p.Id == data.ItemId)) // TODO need better implementation
			{
				HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
				return "Invalid ItemId";
			}

			if (User.Identity is not null && User.Identity.IsAuthenticated &&
				await _userManager.FindByNameAsync(User.Identity.Name) is User user)
			{
				try
				{
					Rate? rate = _context.Rates.FirstOrDefault(r => r.UserId == user.Id && r.ItemId == data.ItemId);
					if (rate is not null)
					{
						_context.Rates.Remove(rate);
						_context.SaveChanges();
						HttpContext.Response.StatusCode = StatusCodes.Status202Accepted;
						return $"Data removed: ItemId={rate.ItemId}, UserId={rate.UserId}, Rating={rate.Rating}";
					}
					else
					{
						HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
						return $"Data doesn't exist: ItemId={data.ItemId}, UserId={user.Id}";
					}
				}
				catch (Exception)
				{
					HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
					return $"Data not processed: ItemId={data?.ItemId}, Type={data?.Type}";
				}
			}
			else
			{
				HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
				return "Not authorized";
			}
		}
	}
}
