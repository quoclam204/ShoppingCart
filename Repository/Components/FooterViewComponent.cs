using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShoppingCart.Repository.Components
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;

        public FooterViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        //public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Brands.ToListAsync());

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var contact = await _dataContext.Contacts.FirstOrDefaultAsync();
            return View(contact);
        }
    }
}
