INSERT INTO public.user_information (user_pk, username, email, "password")
SELECT 1, 'admin', 'testuser1@example.com', 'sausage'
WHERE NOT EXISTS (
  SELECT 1 FROM public.user_information WHERE username = 'admin'
);