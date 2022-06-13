FROM amd64/alpine:3.15

RUN apk add --no-cache \
        ca-certificates \
        \
        # .NET Core dependencies
		icu-libs \
        krb5-libs \
        libgcc \
        libintl \
        libssl1.1 \
        libstdc++ \
        zlib

ENV \
	DOTNET_VERSION=6.0.5 \
	# Enable detection of running in a container
	DOTNET_RUNNING_IN_CONTAINER=true \
	#‎‎不设置固定模式，因为包括 ICU 包和zh-CN内容（请参阅 https://github.com/dotnet/announcements/issues/20）‎
	DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
	
WORKDIR /root/bot
RUN mkdir /root/bot/db
COPY ./RBQBot ./
COPY ./database.db ./db/
ENTRYPOINT ["./RBQBot"]