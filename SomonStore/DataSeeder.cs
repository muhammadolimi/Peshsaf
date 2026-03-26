using SomonStore.Models;

namespace SomonStore
{
    public static class DataSeeder
    {
        public static void Seed(ShopDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.UserRoles.Any())
            {
                context.UserRoles.AddRange(
                    new UserRole { Name = "Admin" },
                    new UserRole { Name = "Customer" });
                context.SaveChanges();
            }

            if (!context.PaymentMethods.Any())
            {
                context.PaymentMethods.AddRange(
                    new PaymentMethod { Name = "Наличные" },
                    new PaymentMethod { Name = "Карта" },
                    new PaymentMethod { Name = "Перевод" });
                context.SaveChanges();
            }

            if (!context.Categories.Any())
            {
                var cameras = new Category { Name = "Камеры" };
                var storage = new Category { Name = "Накопители" };
                var cables = new Category { Name = "Кабели" };
                var monitors = new Category { Name = "Мониторы" };
                context.Categories.AddRange(cameras, storage, cables, monitors);
                context.SaveChanges();

                context.Categories.AddRange(
                    new Category { Name = "IP-камеры", ParentId = cameras.Id },
                    new Category { Name = "AHD-камеры", ParentId = cameras.Id },
                    new Category { Name = "SSD", ParentId = storage.Id },
                    new Category { Name = "HDD", ParentId = storage.Id });
                context.SaveChanges();
            }

            var adminRole = context.UserRoles.First(r => r.Name == "Admin");
            var customerRole = context.UserRoles.First(r => r.Name == "Customer");

            if (!context.Users.Any(u => u.Login == "admin"))
            {
                context.Users.Add(new User
                {
                    Login = "admin",
                    Password = "123",
                    Email = "admin@somonstore.local",
                    Name = "Главный администратор",
                    Phone = "+992900000000",
                    Address = "Khujand",
                    BonusBalance = 0,
                    RoleId = adminRole.Id
                });
            }

            if (!context.Users.Any(u => u.Login == "user"))
            {
                context.Users.Add(new User
                {
                    Login = "user",
                    Password = "123",
                    Email = "user@somonstore.local",
                    Name = "Тестовый пользователь",
                    Phone = "+992911111111",
                    Address = "Khujand",
                    BonusBalance = 50,
                    RoleId = customerRole.Id
                });
            }

            context.SaveChanges();

            foreach (var seededUser in context.Users.ToList())
            {
                if (!context.Carts.Any(c => c.UserId == seededUser.Id))
                {
                    context.Carts.Add(new Cart { UserId = seededUser.Id });
                }
            }
            context.SaveChanges();

            if (!context.Products.Any())
            {
                var ipCamera = context.Categories.First(c => c.Name == "IP-камеры");
                var ahdCamera = context.Categories.First(c => c.Name == "AHD-камеры");
                var ssd = context.Categories.First(c => c.Name == "SSD");
                var hdd = context.Categories.First(c => c.Name == "HDD");
                var cables = context.Categories.First(c => c.Name == "Кабели");
                var monitors = context.Categories.First(c => c.Name == "Мониторы");

                context.Products.AddRange(
                    new Product
                    {
                        Name = "Hikvision DS-2CD1043G2-LIU",
                        Description = "IP-камера 4MP, ночное видение, защита IP67.",
                        Price = 890,
                        Quantity = 12,
                        CategoryId = ipCamera.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=900&q=80"
                    },
                    new Product
                    {
                        Name = "Dahua HAC-HFW1200TLP",
                        Description = "Уличная AHD-камера Full HD для базовой системы наблюдения.",
                        Price = 520,
                        Quantity = 18,
                        CategoryId = ahdCamera.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1557324232-b8917d3c3dcb?auto=format&fit=crop&w=900&q=80"
                    },
                    new Product
                    {
                        Name = "Samsung 970 EVO Plus 1TB",
                        Description = "Быстрый NVMe SSD для ПК и ноутбуков.",
                        Price = 760,
                        Quantity = 9,
                        CategoryId = ssd.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1591488320449-011701bb6704?auto=format&fit=crop&w=900&q=80"
                    },
                    new Product
                    {
                        Name = "Seagate SkyHawk 2TB",
                        Description = "Жесткий диск для видеонаблюдения с круглосуточной нагрузкой.",
                        Price = 610,
                        Quantity = 14,
                        CategoryId = hdd.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1531492746076-161ca9bcad58?auto=format&fit=crop&w=900&q=80"
                    },
                    new Product
                    {
                        Name = "HDMI 2.0 кабель 3м",
                        Description = "Надежный кабель для мониторов, ТВ и видеорегистраторов.",
                        Price = 45,
                        Quantity = 40,
                        CategoryId = cables.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1587202372775-e229f172b9d7?auto=format&fit=crop&w=900&q=80"
                    },
                    new Product
                    {
                        Name = "AOC 24B2XH 24\"",
                        Description = "Тонкий монитор Full HD для офиса и дома.",
                        Price = 980,
                        Quantity = 6,
                        CategoryId = monitors.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=900&q=80"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
