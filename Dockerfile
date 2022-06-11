FROM amd64/alpine:3.15

RUN apk add --no-cache \
        ca-certificates \
        \
        # .NET Core dependencies
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
	#‎‎设置固定模式，因为不包括 ICU 包（请参阅 https://github.com/dotnet/announcements/issues/20）‎
	DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
	
WORKDIR /app
ENTRYPOINT ["./RBQBot"]