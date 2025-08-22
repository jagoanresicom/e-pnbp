# Use Mono runtime as base image for ASP.NET MVC
FROM mono:latest

# Set working directory
WORKDIR /app

# Install necessary packages
RUN apt-get update && apt-get install -y \
    wget \
    unzip \
    curl \
    supervisor \
    nginx \
    && rm -rf /var/lib/apt/lists/*

# Install Oracle Instant Client (required for Oracle connectivity)
RUN wget https://download.oracle.com/otn_software/linux/instantclient/1919000/instantclient-basic-linux.x64-19.19.0.0.0dbru.zip -O /tmp/oracle-client.zip && \
    unzip /tmp/oracle-client.zip -d /opt/ && \
    mv /opt/instantclient_19_19 /opt/oracle && \
    rm /tmp/oracle-client.zip

# Set Oracle environment variables
ENV ORACLE_HOME=/opt/oracle
ENV LD_LIBRARY_PATH=$ORACLE_HOME:$LD_LIBRARY_PATH
ENV PATH=$ORACLE_HOME:$PATH

# Copy application files
COPY . /app/

# Copy NuGet packages (if not using package restore)
COPY packages/ /app/packages/

# Build the application
RUN xbuild /app/Pnbp.sln /p:Configuration=Release /p:Platform="Any CPU"

# Configure Nginx for reverse proxy
COPY docker/nginx.conf /etc/nginx/sites-available/default

# Configure Supervisor for process management
COPY docker/supervisord.conf /etc/supervisor/conf.d/supervisord.conf

# Create startup script
RUN echo '#!/bin/bash\n\
echo "Starting PNBP Application..."\n\
echo "Oracle Home: $ORACLE_HOME"\n\
echo "LD_LIBRARY_PATH: $LD_LIBRARY_PATH"\n\
\n\
# Start the Mono application server\n\
cd /app\n\
exec xsp4 --port=9000 --root=/app --applications=/:. --nonstop\n\
' > /app/start-app.sh && chmod +x /app/start-app.sh

# Expose ports
EXPOSE 80 9000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:9000/ || exit 1

# Start supervisor to manage processes
CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]