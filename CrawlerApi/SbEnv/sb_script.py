from seleniumbase import SB
import sys
import codecs

# ensures that the script can handle UTF-8 encoded output, which is important for non-ASCII characters like emojis
sys.stdout = codecs.getwriter('utf-8')(sys.stdout.detach())
sys.stderr = codecs.getwriter('utf-8')(sys.stderr.detach())

def url_from_csharp():
    # sys.argv is the list of command line arguments passed to the script through the python.exe
    if len(sys.argv) > 1:
        input_data = sys.argv[1] #argv[0] is a pointer to the script name, not useful at all
        try:
            if (input_data is not None):
                url = input_data
                return url
            else:
                sys.stderr.write("Error in Python: No input data provided via command line arguments.\n")
                sys.exit(1)      
        except Exception as e:
            sys.stderr.write(f"Error in Python: An unexpected error occurred during processing. Details: {e}\n")
            sys.exit(1)
            
    else:
        # sends the error message to the error stream and exits the code without crashing and causing any problems
        sys.stderr.write("Error in Python: No input data provided via command line arguments.\n")
        sys.exit(1) # exit with aproblem
    
def skip_captcha(url):
    with SB(uc=True, ad_block_on=True) as sb:
        sb.uc_open_with_reconnect(url, 6)
        sb.uc_gui_click_captcha()
        html_content = sb.get_page_source()
        return html_content

if __name__ == '__main__': # means that this code will only run if the script is executed directly, not when imported
    url = url_from_csharp()

    html_content = skip_captcha(url)
    
    sys.stdout.write(html_content)
    sys.exit(0) # exit with success