#!/usr/bin/env ruby
# ทดสอบ UploadPayInSlipById พร้อม first4/last4/note (AllowAnonymous — ไม่ต้อง token)
# ขั้นตอน:
#   1. รัน test-pmr-payin-generate-slip-token.rb ก่อนเพื่อได้ token
#   2. ใส่ token + paymentRequestId ด้านล่าง แล้วรัน script นี้

require 'net/http'
require 'uri'
require 'json'
require 'base64'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId            = "ppm-alfa999"
paymentRequestId = "3fa85f64-5717-4562-b3fc-2c963f66afa6"
slipToken        = "your-slip-token-here"   # ได้จาก GeneratePayInSlipUploadToken

# รูปทดสอบ 1x1 pixel JPEG (แทน screenshot จริง)
tinyJpeg = "/9j/4AAQSkZJRgABAQEASABIAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0a" \
           "HBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/wAARCAABAAEDASIAAhEBAxEB/8QAFgAB" \
           "AQEAAAAAAAAAAAAAAAAABgUEB//EAB0QAAICAgMAAAAAAAAAAAAAAAABAgMEERITIf/EABQBAQAAAA" \
           "AAAAAAAAAAAAAAAAAD/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwCwzkWpNJdmFqkZ" \
           "3C7s7TE5gAB//9k="

###
apiUrl = "api/PaymentRequest/org/#{orgId}/action/UploadPayInSlipById/#{paymentRequestId}/#{slipToken}"
param = {
  ImageBase64: tinyJpeg,
  First4: "AA11",
  Last4:  "ZZ99",
  Note:   "ทดสอบ first4/last4/note",
}

ENV['API_KEY'] = nil      # AllowAnonymous
ENV['ACCESS_TOKEN'] = nil

result = make_request(:post, apiUrl, param)
puts(result.to_json)
