using System;

namespace FacturaLuz.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);
}
