FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app
COPY . /app

ENV TZ Asia/Shanghai
#ENV ASPNETCORE_APP_PATH "/safetyservice"
#修改apt-get源，加速apt下载
RUN sed -i s@/deb.debian.org/@/mirrors.163.com/@g /etc/apt/sources.list
RUN cat /etc/apt/sources.list
#安装fontconfig
RUN apt-get clean
RUN apt-get update && apt-get install -y fontconfig

EXPOSE 80
ENTRYPOINT ["dotnet", "CimcSite.Web.dll"]
