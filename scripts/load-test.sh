#!/usr/bin/env bash
set -euo pipefail

API_URL="${API_URL:-http://localhost:5133}"
LISTENER_URL="${LISTENER_URL:-http://localhost:5200}"
TOTAL=10000
CONCURRENCY=100

# 1. Register subscription
echo "Registering subscription for order.created → $LISTENER_URL/webhooks"
curl -sf -X POST "$API_URL/api/webhooks/subscribtions" \
    -H "Content-Type: application/json" \
    -d "{\"WebhookUrl\":\"$LISTENER_URL/webhooks\",\"EventType\":\"order.created\"}" \
    | cat
echo

# 2. Send orders
echo "Sending $TOTAL orders ($CONCURRENCY concurrent)..."
START=$(date +%s)

seq 1 $TOTAL | xargs -P $CONCURRENCY -I {} curl -s -o /dev/null -w "%{http_code}\n" \
    -X POST "$API_URL/api/orders" \
    -H "Content-Type: application/json" \
    -d '{"CustomerName":"LoadTestUser","Amount":99.99}' | sort | uniq -c

echo "All orders submitted. Waiting for pipeline to finish..."

# 3. Poll until delivery attempt id=TOTAL exists
TIMEOUT=120
ELAPSED=0
while true; do
    LAST=$(curl -sf "$API_URL/api/webhooks/delivery-attempts/$TOTAL?success=true")
    if echo "$LAST" | grep -q "\"id\":$TOTAL"; then
        break
    fi
    if [ "$ELAPSED" -ge "$TIMEOUT" ]; then
        echo "Timed out after ${TIMEOUT}s waiting for delivery attempt $TOTAL"
        # Show TestListener stats for diagnostics
        curl -sf "$LISTENER_URL/stats" | cat
        echo
        exit 1
    fi
    sleep 2
    ELAPSED=$((ELAPSED + 2))
done

# 4. Fetch first and last delivery attempts, compute duration from createdAt
FIRST=$(curl -sf "$API_URL/api/webhooks/delivery-attempts/1?success=true")
LAST=$(curl -sf "$API_URL/api/webhooks/delivery-attempts/$TOTAL?success=true")

FIRST_AT=$(echo "$FIRST" | grep -oP '(?<="createdAt":")[^"]+')
LAST_AT=$(echo "$LAST"  | grep -oP '(?<="createdAt":")[^"]+')

FIRST_TS=$(date -d "$FIRST_AT" +%s)
LAST_TS=$(date -d "$LAST_AT" +%s)

echo
echo "First delivery attempt createdAt : $FIRST_AT"
echo "Last  delivery attempt createdAt : $LAST_AT"
echo "Pipeline duration (first → last) : $((LAST_TS - FIRST_TS))s"
echo "Total wall time   (submit → last): $((LAST_TS - START))s"
